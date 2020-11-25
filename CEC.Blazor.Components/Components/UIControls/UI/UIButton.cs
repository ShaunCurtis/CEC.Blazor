using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a row
    ///  Provides a structured  mechanism for managing Bootstrap class elements used in Editors and Viewers in one place. 
    /// The properties are pretty self explanatory and therefore not decorated with summaries
    /// </summary>

    public class UIButton : UIBootstrapBase
    {
        /// <summary>
        /// Property setting the button HTML attribute Type
        /// </summary>
        [Parameter]
        public string ButtonType { get; set; } = "button";

        /// <summary>
        /// Override the CssName
        /// </summary>
        protected override string CssName => "btn";

        /// <summary>
        /// Override the Tag
        /// </summary>
        protected override string _Tag => "button";

        /// <summary>
        /// Callback for a button click event
        /// </summary>
        [Parameter]
        public EventCallback<MouseEventArgs> ClickEvent { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.Show)
            {
                builder.OpenElement(0, this._Tag);
                builder.AddAttribute(1, "type", this.ButtonType);
                builder.AddAttribute(2, "class", this._Css);
                builder.AddAttribute(3, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, this.ButtonClick));
                builder.AddContent(4, ChildContent);
                builder.CloseElement();
            }
        }

        /// <summary>
        /// Event handler for button click
        /// </summary>
        /// <param name="e"></param>
        protected void ButtonClick(MouseEventArgs e) => this.ClickEvent.InvokeAsync(e);
    }
}
