using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Blazor.Components.UIControls
{
    public class UICardEditButtonColumn : UITDColumn
    {
        [Parameter]
        public bool IsHeader { get; set; }

        protected override string _Css => $"column-normal text-right";

        protected override bool _Show => this.UIOptions?.ShowEdit ?? true;

        protected override string _Tag => this.IsHeader ? "th" : "td";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.Show)
            {
                int i = 0;
                builder.OpenElement(i, this._Tag);
                builder.AddAttribute(i++, "class", this._Css);
                if (!this.IsHeader)
                {
                    builder.OpenElement(i, "a");
                    builder.AddAttribute(i++, "class", "badge badge-primary p-2 text-white cursor-hand");
                    builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.Card.NavigateToEditor(this.RecordID))));
                    builder.AddContent(i++, "Edit");
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
