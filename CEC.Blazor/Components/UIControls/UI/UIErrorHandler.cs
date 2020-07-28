using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    public class UIErrorHandler : UIBase
    {
        [Parameter]
        public bool IsError { get; set; } = false;

        [Parameter]
        public bool IsLoading { get; set; } = true;

        [Parameter]
        public string ErrorMessage { get; set; } = "Loading....";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (IsLoading && IsError)
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(2, "class", "text-center p-3");
                builder.OpenElement(1, "button");
                builder.AddAttribute(2, "class", "btn btn-primary");
                builder.AddAttribute(3, "type", "button");
                builder.AddAttribute(4, "disabled", "disabled");
                builder.OpenElement(5, "span");
                builder.AddAttribute(6, "class", "spinner-border spinner-border-sm pr-2");
                builder.AddAttribute(2, "role", "status");
                builder.AddAttribute(2, "aria-hidden", "true");
                builder.CloseElement();
                builder.AddContent(3, "  Loading...");
                builder.CloseElement();
                builder.CloseElement();
            }
            else if (IsError)
            {
                builder.OpenElement(0, "div");
                builder.OpenElement(1, "span");
                builder.AddAttribute(2, "class", "label label-error m-2");
                builder.AddContent(3, ErrorMessage);
                builder.CloseElement();
                builder.CloseElement();
            }
            else builder.AddContent(0, ChildContent);
        }
    }
}
