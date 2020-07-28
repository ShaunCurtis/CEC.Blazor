
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a grid table
    ///  Provides a structured mechanism for building UI Compoenents
    /// The properties are pretty self explanatory and therefore not decorated with summaries
    /// </summary>

    public class UIGridTableContainer : UIBase
    {
        [Parameter]
        public int MaxColumn { get; set; } = 1;

        protected override string _Css => $"grid-table {this.AddOnCss}".Trim();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int i = 0;
            builder.OpenElement(i, "div");
            builder.AddAttribute(i++, "class", this._Css);
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
