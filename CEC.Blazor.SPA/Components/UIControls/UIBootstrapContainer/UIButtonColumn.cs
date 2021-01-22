
namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Bootstrap Button Column
    /// </summary>

    public class UIButtonColumn : UIColumn
    {
        protected override string _BaseCss => $"col-{Columns} text-right";

    }
}
