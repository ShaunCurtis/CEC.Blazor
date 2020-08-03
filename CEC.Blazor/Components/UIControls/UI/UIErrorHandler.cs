using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    public class UIErrorHandler : UIComponent
    {
        [Parameter]
        public bool IsError { get; set; } = false;

        [Parameter]
        public bool IsLoading { get; set; } = true;

        protected override string _BaseCss => this.IsLoading? "text-center p-3": "label label-error m-2";

        [Parameter]
        public string ErrorMessage { get; set; } = "Loading....";

        
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            this.ClearDuplicateAttributes();
            var i = -1;
            if (IsLoading && IsError)
            {
                builder.OpenElement(i++, "div");
                builder.AddAttribute(i++, "class", this._Css);
                builder.OpenElement(i++, "button");
                builder.AddAttribute(i++, "class", "btn btn-primary");
                builder.AddAttribute(i++, "type", "button");
                builder.AddAttribute(i++, "disabled", "disabled");
                builder.OpenElement(i++, "span");
                builder.AddAttribute(i++, "class", "spinner-border spinner-border-sm pr-2");
                builder.AddAttribute(i++, "role", "status");
                builder.AddAttribute(i++, "aria-hidden", "true");
                builder.CloseElement();
                builder.AddContent(i++, "  Loading...");
                builder.CloseElement();
                builder.CloseElement();
            }
            else if (IsError)
            {
                builder.OpenElement(i++, "div");
                builder.OpenElement(i++, "span");
                builder.AddAttribute(i++, "class", this._Css);
                builder.AddContent(i++, ErrorMessage);
                builder.CloseElement();
                builder.CloseElement();
            }
            else builder.AddContent(i++, ChildContent);
        }
    }
}
