using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a row
    /// </summary>

    public class UIRow : UIBase
    {
        protected override string _Css => $"row {AddOnCss.Trim()}".Trim();
    }
}
