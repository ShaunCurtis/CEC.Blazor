using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// Base UI Rendering Wrapper to build a Bootstrap HTML Tag
    /// </summary>

    public class UITag : UIBase
    {
        /// <summary>
        /// Property to set the Tag of the HTML element
        /// </summary>
        [Parameter]
        public virtual string Tag { get; set; } = "div";

        /// <summary>
        /// Override this Protected property to set the Tag in inherited classes.  Default implementation sets it to the Tag Parameter Property set by the user (default is div)
        /// </summary>
        protected override string _Tag => this.Tag;

    }
}
