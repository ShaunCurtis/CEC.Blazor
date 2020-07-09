
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a row
    ///  Provides a structured  mechanism for managing Bootstrap class elements used in Editors and Viewers in one place. 
    /// The properties are pretty self explanatory and therefore not decorated with summaries
    /// </summary>

    public class UIGridContainer : UIBase
    {
        [Parameter]
        public int MaxColumn { get; set; } = 1;

        protected override string Css => $"grid-container grid-max-{this.MaxColumn}";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int i = 0;
            builder.OpenElement(i, "div");
            builder.AddAttribute(i++, "class", this.Css);
            if (!string.IsNullOrEmpty(this.ComponentId)) builder.AddAttribute(i++, "id", this.ComponentId);
            builder.OpenComponent<CascadingValue<int>>(i++);
            builder.AddAttribute(i++, "Name", "MaxColumn");
            builder.AddAttribute(i++, "Value", this.MaxColumn);
            if (this.ChildContent != null) builder.AddAttribute(i++, "ChildContent", ChildContent);
            builder.CloseComponent();
            builder.CloseElement();
        }
    }
}
