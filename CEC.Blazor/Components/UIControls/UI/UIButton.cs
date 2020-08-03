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

    public class UIButton : UIBase
    {
        /// <summary>
        /// Property setting the button HTML attribute Type
        /// </summary>
        [Parameter]
        public string ButtonType { get; set; } = "button";

        /// <summary>
        /// Property to set the Bootstrap button size
        /// </summary>
        [Parameter]
        public string ButtonFlavor { get; set; } = "";

        /// <summary>
        /// Property to set the Bootstrap colour
        /// </summary>
        [Parameter]
        public string ButtonColour { get; set; } = "btn-primary";

        /// <summary>
        /// Property to set the HTML value
        /// </summary>
        [Parameter]
        public string Value { get; set; } = "";

        /// <summary>
        /// Callback for a button click event
        /// </summary>
        [Parameter]
        public EventCallback<MouseEventArgs> ClickEvent { get; set; }

        /// <summary>
        /// Override of the CSS
        /// </summary>
        protected override string _Css => this.CleanUpCss($"btn {ButtonFlavor} {ButtonColour} {AddOnCss}");

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.Show)
            {
                var i = -1;
                builder.OpenElement(i++, "button");
                builder.AddAttribute(i++, "type", this.ButtonType);
                builder.AddAttribute(i++, "class", this._Css);
                builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, this.ButtonClick));
                builder.AddContent(i++, ChildContent);
                builder.CloseElement();
            }
        }

        /// <summary>
        /// Event handler for button click
        /// </summary>
        /// <param name="e"></param>
        protected void ButtonClick(MouseEventArgs e) => this.ClickEvent.InvokeAsync(e);

        #region CSS Constants

        public static class Colour
        {
            public static string BtnPrimary = "btn-primary";
            public static string BtnSecondary = "btn-secondary";
            public static string BtnDanger = "btn-danger";
            public static string BtnWarning = "btn-warning";
            public static string BtnSuccess = "btn-success";
            public static string BtnDark = "btn-dark";
            public static string BtnLight = "btn-light";
            public static string BtnInfo = "btn-info";
            public static string BtnAdd = "btn-add";
            public static string BtnEdit = "btn-edit";
            public static string BtnDelete = "btn-delete";
            public static string BtnCancel = "btn-cancel";
            public static string BtnNav = "btn-nav";
            public static string BtnOutlinePrimary = "btn-outline-primary";
            public static string BtnOutlineSecondary = "btn-outline-secondary";
            public static string BtnOutlineDanger = "btn-outline-danger";
            public static string BtnOutlineWarning = "btn-outline-warning";
            public static string BtnOutlineSuccess = "btn-outline-success";
            public static string BtnOutlineDark = "btn-outline-dark";
            public static string BtnOutlineLight = "btn-outline-light";
            public static string BtnOutlineInfo = "btn-outline-info";

        }
        
        public static class Size
        {
            public static string BtnLarge = "btn-lg";
            public static string BtnSmall = "btn-sm";
        }

        #endregion
    }
}
