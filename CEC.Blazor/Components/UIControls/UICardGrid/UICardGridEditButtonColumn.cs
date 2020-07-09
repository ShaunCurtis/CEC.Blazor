using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Blazor.Components.UIControls
{
    public class UICardGridEditButtonColumn : UIGridColumn
    {
        [CascadingParameter]
        public UICardGridBase CardGrid { get; set; }

        protected override string Css => $"grid-col-{this.Column}{this.CssHeader}{this.AltRowCss} grid-col-right";

        protected bool Show => this.UIOptions?.ShowEdit ?? true;

        protected string AltRowCss => this.CardGrid.AltRow ? "grid-alt-row" : string.Empty;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.Show)
            {
                int i = 0;
                builder.OpenElement(i, "div");
                builder.AddAttribute(i++, "class", this.Css);
                if (!string.IsNullOrEmpty(this.ComponentId)) builder.AddAttribute(i++, "id", this.ComponentId);
                if (!this.IsHeader)
                {
                    builder.OpenElement(i, "a");
                    builder.AddAttribute(i++, "class", "badge badge-primary p-2 text-white cursor-hand");
                    builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.CardGrid.NavigateToEditor())));
                    builder.CloseElement();
                }
                else
                {
                    builder.AddContent(i++, (MarkupString)"&nbsp;");
                }
                builder.CloseElement();
            }
        }
    }
}
