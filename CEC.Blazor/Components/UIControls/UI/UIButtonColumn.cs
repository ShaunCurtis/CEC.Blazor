using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Column
    ///  Provides a structured  mechanism for managing Bootstrap class elements used in Editors and Viewers in one place. 
    /// The properties are pretty self explanatory and therefore not decorated with summaries
    /// </summary>

    public class UIButtonColumn : UIColumnBase
    {
        protected override string Css => $"col-{Columns} text-right pb-3 {this.FormGroup} {AddOnCss.Trim()}".Trim();

    }
}
