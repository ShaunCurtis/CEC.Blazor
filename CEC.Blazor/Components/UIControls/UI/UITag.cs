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
        /// Protected property to override the Tag in inherited classes.  By default it is set to the Tag Parameter Property
        /// </summary>
        protected override string _Tag => this.Tag;

    }
}
