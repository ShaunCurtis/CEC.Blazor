
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Grid Row - currently intentionally empty!
    /// Using the CSS Grid System this is not neccessary, but added as we may at some point move to a different grid system where rows are used
    /// </summary>

    public class UIGridRow : UIBase
    {
        protected override string Css => string.Empty;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            // Do nothing as we don't need this at the moment on the Grid System
        }
    }
}
