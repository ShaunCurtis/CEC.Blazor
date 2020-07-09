
namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Grid Column
    /// </summary>

    public class UIGridHeaderColumn : UIGridColumn
    {
        protected override void OnInitialized()
        {
            this.IsHeader = true;
            base.OnInitialized();
        }
    }
}
