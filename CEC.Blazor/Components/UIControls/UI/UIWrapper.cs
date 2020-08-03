using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper for UI Cascading
    /// </summary>

    public class UIWrapper : UIBase
    {
        /// <summary>
        /// UIOptions object to cascade
        /// </summary>
        [Parameter]
        public UIOptions UIOptions { get; set; } = new UIOptions();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int i = -1;
            builder.OpenComponent<CascadingValue<UIOptions>>(i++);
            builder.AddAttribute(i++, "Value", this.UIOptions);
            if (this.ChildContent != null) builder.AddAttribute(i++, "ChildContent", ChildContent);
            builder.CloseComponent();
        }
    }
}
