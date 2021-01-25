using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.SPA.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Bootstrap Column
    /// </summary>

    public class UIColumn : UIBase
    {
        [Parameter]
        public int Columns { get; set; } = 1;

        protected override string _BaseCss => $"col-{Columns}";
    }
}
