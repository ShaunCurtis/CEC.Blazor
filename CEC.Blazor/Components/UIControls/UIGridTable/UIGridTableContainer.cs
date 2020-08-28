
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a grid table
    ///  Provides a structured mechanism for building UI Compoenents
    /// The properties are pretty self explanatory and therefore not decorated with summaries
    /// </summary>

    public class UIGridTableContainer : UIComponent
    {
        [Parameter]
        public int MaxColumn { get; set; } = 1;

        protected override string _Tag => "table";

        protected override string _BaseCss => "grid-table";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            this.ClearDuplicateAttributes();
            int i = -1;
            builder.OpenElement(i++, this._Tag);
            builder.AddMultipleAttributes(i++, AdditionalAttributes);
            builder.AddAttribute(i++, "class", this._Css);
            builder.OpenComponent<CascadingValue<int>>(i++);
            builder.AddAttribute(i++, "Name", "MaxColumn");
            builder.AddAttribute(i++, "Value", this.MaxColumn);
            if (this.ChildContent != null) builder.AddAttribute(i++, "ChildContent", ChildContent);
            builder.CloseComponent();
            builder.CloseElement();
        }
    }
}
