
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Grid Column
    /// </summary>

    public class UIGridTableColumn : UIBase
    {

        [CascadingParameter(Name = "MaxColumn")]
        public int MaxColumn { get; set; } = 1;

        [Parameter]
        public int Column { get; set; } = 1;

        [Parameter]
        public int ColumnSpan { get; set; }

        [Parameter]
        public ColumnAlignment Alignment { get; set; } = ColumnAlignment.Left;

        protected override string _Css => $"grid-col {this.CssAlignment}{this.CssHeader}{this.MaxRowCss} {this.AddOnCss}".Trim();

        [Parameter]
        public bool IsHeader { get; set; }

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
            int i = 0;
            builder.OpenElement(i, "td");
            builder.AddAttribute(i++, "class", this._Css);
            if (this.ColumnSpan > 1) builder.AddAttribute(i++, "colspan", this.ColumnSpan);
            if (this.IsMaxColumn)
            {
                builder.OpenElement(i++, "div");
                builder.AddAttribute(i++, "class", "grid-overflow");
                builder.OpenElement(i++, "div");
                builder.AddAttribute(i++, "class", "grid-overflowinner");
                builder.AddContent(i++, ChildContent);
                builder.CloseElement();
                builder.CloseElement();
            }
            else
            {
                builder.AddContent(i++, ChildContent);
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
