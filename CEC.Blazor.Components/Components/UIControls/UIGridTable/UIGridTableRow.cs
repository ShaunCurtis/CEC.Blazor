
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Grid Row 
    /// </summary>

    public class UIGridTableRow : UIBase
    {

        protected override string _Tag => "tr";


        protected override string _Css => $"grid-row {AddOnCss.Trim()}".Trim();

    }
}
