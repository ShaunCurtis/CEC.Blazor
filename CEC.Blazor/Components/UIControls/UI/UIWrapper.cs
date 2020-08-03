using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper for UI Cascading
    /// </summary>

    public class UIWrapper : UIBase
    {
        /// <summary>
        /// UIOptions object to cascade
        /// </summary>
        [Parameter]
        public UIOptions UIOptions { get; set; } = new UIOptions();

        /// <summary>
        /// OnView Action Delegate to cascade
        /// </summary>
        [Parameter]
        public Action<int> OnView { get; set; }

        /// <summary>
        /// OnEdit Action Delegate to cascade
        /// </summary>
        [Parameter]
        public Action<int> OnEdit { get; set; }

        /// <summary>
        /// UIOptions object to cascade
        /// </summary>
        [Parameter]
        public RecordConfigurationData RecordConfiguration { get; set; } = new RecordConfigurationData();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int i = -1;
            builder.OpenComponent<CascadingValue<UIWrapper>>(i++);
            builder.AddAttribute(i++, "Value", this);
            if (this.ChildContent != null) builder.AddAttribute(i++, "ChildContent", ChildContent);
            builder.CloseComponent();
        }
    }
}
