
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
        /// <summary>
        /// Adjustable Column - css width set to max
        /// </summary>
        [Parameter]
        public int MaxColumn { get; set; } = 1;

        protected override string _Tag => "table";

        protected override string _BaseCss => "grid-table";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            this.ClearDuplicateAttributes();
            builder.OpenElement(0, this._Tag);
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "class", this._Css);
            builder.OpenComponent<CascadingValue<int>>(3);
            builder.AddAttribute(4, "Name", "MaxColumn");
            builder.AddAttribute(5, "Value", this.MaxColumn);
            if (this.ChildContent != null) builder.AddAttribute(6, "ChildContent", ChildContent);
            builder.CloseComponent();
            builder.CloseElement();
        }
    }
}
