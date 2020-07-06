using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// Base UI Rendering Wrapper to build a Botstrap Component
    /// </summary>

    public class UIBase : ComponentBase
    {

        [Parameter]
        public string Id { get; set; } = "";

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string AddOnCss { get; set; } = string.Empty;

        [CascadingParameter]
        public UIOptions UIOptions { get; set; } = new UIOptions();

        protected virtual string Css => $"{AddOnCss.Trim()}".Trim();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", this.Css);
            if (!string.IsNullOrEmpty(this.Id)) builder.AddAttribute(1, "id", this.Id);
            builder.AddContent(2, ChildContent);
            builder.CloseElement();
        }
    }
}
