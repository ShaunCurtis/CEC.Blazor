using System;
using System.Globalization;
using CEC.Blazor.Components.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// Builds a Bootstrap View Link
    /// </summary>

    public class UIViewLink : UIBase
    {
        [Parameter] public Type ViewType { get; set; }

        [CascadingParameter] public ViewManager ViewManager { get; set; }

        private bool IsActive => this.ViewManager.IsCurrentView(this.ViewType);

        /// <summary>
        /// inherited
        /// Builds the render tree for the component
        /// </summary>
        /// <param name="builder"></param>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var css = string.Empty;

            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var obj))
            {
                css = Convert.ToString(obj, CultureInfo.InvariantCulture);
            }
            if (this.IsActive) css = $"{css} active";
            this.ClearDuplicateAttributes();
            builder.OpenElement(0, "a");
            builder.AddAttribute(1, "class", css);
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            builder.AddContent(3, ChildContent);
            builder.CloseElement();
        }
    }
}
