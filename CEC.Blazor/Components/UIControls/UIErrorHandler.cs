using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    public class UIErrorHandler : UIBase
    {
        [Parameter]
        public bool IsError { get; set; } = false;

        [Parameter]
        public string ErrorMessage { get; set; } = "loading....";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (IsError)
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
