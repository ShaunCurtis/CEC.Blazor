using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// Base UI Rendering Wrapper to build a Bootstrap Component
    /// </summary>

    public abstract class UIComponent : UIBase
    {

        /// <summary>
        /// Cascaded UIWrapper
        /// </summary>
        [CascadingParameter]
        public UIWrapper UIWrapper { get; set; } = new UIWrapper();

    }
}
