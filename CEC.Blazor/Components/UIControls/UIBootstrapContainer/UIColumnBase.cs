using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// Base UI Rendering Wrapper to build a Botstrap Component
    /// </summary>

    public class UIColumnBase : UIBase
    {
        [Parameter]
        public int Columns { get; set; } = 1;

        protected override string _Css => $"col-{Columns} {AddOnCss.Trim()}".Trim();

    }
}
