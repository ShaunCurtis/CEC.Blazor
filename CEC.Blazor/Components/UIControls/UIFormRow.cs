using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Bootstrap Form Row
    /// </summary>

    public class UIFormRow : UIBase
    {
        protected override string Css => $"row form-group {AddOnCss.Trim()}".Trim();

    }
}
