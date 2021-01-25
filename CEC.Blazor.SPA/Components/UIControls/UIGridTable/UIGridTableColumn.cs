
using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Blazor.SPA.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Grid Column
    /// </summary>

    public class UIGridTableColumn<TRecord> : UIComponent  where TRecord : IDbRecord<TRecord>, new()
    {
        /// <summary>
        /// Cascaded UIGridTable
        /// </summary>
        [CascadingParameter]
        public UICardGrid<TRecord> Card { get; set; }

        /// <summary>
        /// Record ID passed via a cascade
        /// </summary>
        [CascadingParameter(Name = "RecordID")]
        public int RecordID { get; set; } = 0;

        /// <summary>
        /// Max Column No via a cascade
        /// </summary>
        [CascadingParameter(Name = "MaxColumn")]
        public int MaxColumn { get; set; } = 1;

        [Parameter]
        public int Column { get; set; } = 1;

        [Parameter]
        public int ColumnSpan { get; set; }

        [Parameter]
        public ColumnAlignment Alignment { get; set; } = ColumnAlignment.Left;

        [Parameter]
        public bool IsHeader { get; set; }

        protected override string _Tag => "td";

        protected string Style => this.IsMaxColumn ? $"width: {this.UIWrapper?.UIOptions?.MaxColumnPercent ?? 50}%" : string.Empty;

        protected override string _Css => this.CleanUpCss($"grid-col {this.CssAlignment} {this.CssHeader} {this.MaxRowCss} {this.AddOnCss}");

        protected bool IsMaxColumn => this.Column == this.MaxColumn;

        protected string CssHeader => this.IsHeader ? " grid-col-header" : string.Empty;

        protected string MaxRowCss => this.IsMaxColumn ? " grid-col-overflow" : string.Empty;

        protected string CssAlignment => this.Alignment switch
        {
            ColumnAlignment.Centre => " text-center",
            ColumnAlignment.Right => " text-right",
            _ => " text-left",
        };

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, this._Tag);
            builder.AddAttribute(1, "class", this._Css);
            if (!string.IsNullOrEmpty(this.Style)) builder.AddAttribute(2, "style", this.Style);
            if (this.ColumnSpan > 1) builder.AddAttribute(3, "colspan", this.ColumnSpan);
            if (this.RecordID > 0 && this.Card != null) builder.AddAttribute(4, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.Card.NavigateToView(this.RecordID))));
            if (this.IsMaxColumn)
            {
                builder.OpenElement(5, "div");
                builder.AddAttribute(6, "class", "grid-overflow");
                builder.OpenElement(7, "div");
                builder.AddAttribute(8, "class", "grid-overflowinner");
                builder.AddContent(9, ChildContent);
                builder.CloseElement();
                builder.CloseElement();
            }
            else
            {
                builder.AddContent(10, ChildContent);
            }
            builder.CloseElement();
        }

        public enum ColumnAlignment
        {
            Left,
            Centre,
            Right
        }
    }
}
