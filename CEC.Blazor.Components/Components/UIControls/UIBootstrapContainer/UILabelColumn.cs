using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Bootstrap Label Column
    /// </summary>

    public class UILabelColumn : UIColumn
    {
        protected override string _BaseCss => $"col-{Columns} col-form-label";
    }
}
