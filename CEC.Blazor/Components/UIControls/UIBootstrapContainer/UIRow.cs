using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Bootstrap Row
    /// </summary>

    public class UIRow : UIBootstrapBase
    {
        protected override string _BaseCss => "row";
    }
}
