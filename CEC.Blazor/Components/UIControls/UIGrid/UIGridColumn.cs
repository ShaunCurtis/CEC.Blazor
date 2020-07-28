
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Grid Column
    /// </summary>

    public class UIGridColumn : UIBase
    {

        [CascadingParameter(Name = "MaxColumn")]
        public int MaxColumn { get; set; } = 1;

        [Parameter]
        public int Column { get; set; } = 1;

        public ColumnAlignment Alignment { get; set; } = ColumnAlignment.Left;

        protected override string _Css => $"grid-col grid-col-{this.Column}{this.CssAlignment}{this.CssHeader}";

        [Parameter]
        public bool IsHeader { get; set; }

        protected bool IsMaxColumn => this.Column == this.MaxColumn;

        protected string CssHeader => this.IsHeader ? " grid-col-header" : string.Empty;

        protected string CssAlignment => this.Alignment switch
        {
            ColumnAlignment.Centre => " grid-col-centre",
            ColumnAlignment.Right => " grid-col-right",
            _ => " grid-col-left",
        };

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.MaxColumn == this.Column)
            {
                int i = 0;
                builder.OpenElement(i, "div");
                builder.AddAttribute(i++, "class", this._Css);
                if (!string.IsNullOrEmpty(this.ComponentId)) builder.AddAttribute(i++, "id", this.ComponentId);
                if (this.IsHeader)
                {
                    builder.OpenElement(i++, "div");
                    builder.AddAttribute(i++, "class", "overflow");
                    builder.AddContent(i++, ChildContent);
                    builder.CloseElement();
                }
                else
                {
                    builder.AddContent(i++, ChildContent);
                }
                builder.CloseElement();
            }
            else base.BuildRenderTree(builder);
        }

        public enum ColumnAlignment
        {
            Left,
            Centre,
            Right
        }
    }
}
