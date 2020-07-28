
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Grid Column
    /// </summary>

    public class UIGridTableHeaderColumn : UIGridTableColumn
    {
        protected override void OnInitialized()
        {
            this.IsHeader = true;
            base.OnInitialized();
        }
    }
}
