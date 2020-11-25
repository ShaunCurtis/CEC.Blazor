using System;
using System.Collections.Generic;
using System.Globalization;
using CEC.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// Builds a Bootstrap View Link
    /// </summary>
    public class UIViewLink : UIBase
    {
        /// <summary>
        /// View Type to Load
        /// </summary>
        [Parameter] public Type ViewType { get; set; }

        /// <summary>
        /// View Paremeters for the View
        /// </summary>
        [Parameter] public Dictionary<string, object> ViewParameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Cascaded ViewManager
        /// </summary>
        [CascadingParameter] public ViewManager ViewManager { get; set; }

        /// <summary>
        /// Boolean to check if the ViewType is the current loaded view
        /// if so it's used to mark this component's CSS with "active" 
        /// </summary>
        private bool IsActive => this.ViewManager.IsCurrentView(this.ViewType);

        /// <summary>
        /// inherited
        /// Builds the render tree for the component
        /// </summary>
        /// <param name="builder"></param>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var css = string.Empty;
            var viewData = new ViewData(ViewType, ViewParameters);

            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var obj))
            {
                css = Convert.ToString(obj, CultureInfo.InvariantCulture);
            }
            if (this.IsActive) css = $"{css} active";
            this.UsedAttributes.Add("@onclick");
            this.UsedAttributes.Add("onclick");
            this.ClearDuplicateAttributes();
            builder.OpenElement(0, "a");
            builder.AddAttribute(1, "class", css);
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            //builder.AddAttribute(3, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, e => this.ViewManager.LoadViewAsync(viewData)));
            builder.AddAttribute(3, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, e => this.ViewManager.LoadViewAsync(viewData)));
            builder.AddContent(4, ChildContent);
            builder.CloseElement();
        }
    }
}
