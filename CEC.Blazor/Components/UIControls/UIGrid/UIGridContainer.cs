
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Bootstrap Container
    /// </summary>

    public class UIGridContainer : UIBase
    {
        [Parameter]
        public int MaxColumn { get; set; } = 1;

        protected override string _BaseCss => $"grid-container grid-max-{this.MaxColumn}";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            this.ClearDuplicateAttributes();
            int i = 0;
            builder.OpenElement(i, "div");
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
