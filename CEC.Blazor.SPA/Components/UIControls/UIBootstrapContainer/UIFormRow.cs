using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.SPA.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Bootstrap Form Row
    /// </summary>

    public class UIFormRow : UIBase
    {
        protected override string _BaseCss => $"row form-group";

    }
}
