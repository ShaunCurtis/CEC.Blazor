using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// Base UI Rendering Wrapper to build a Bootstrap Component
    /// </summary>

    public abstract class UIComponent : UIBase
    {

        /// <summary>
        /// Cascaded UIOptions object to act as a container for finer ui control attributes/properties
        /// </summary>
        [CascadingParameter]
        public UIOptions UIOptions { get; set; } = new UIOptions();

    }
}
