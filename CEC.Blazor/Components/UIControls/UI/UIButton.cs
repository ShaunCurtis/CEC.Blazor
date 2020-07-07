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
        [Parameter]
        public string ButtonType { get; set; } = "button";

        [Parameter]
        public string ButtonFlavor { get; set; } = "";

        [Parameter]
        public string ButtonColour { get; set; } = "btn-primary";

        [Parameter]
        public string Value { get; set; } = "";

        [Parameter]
        public bool Show { get; set; } = true;

        [Parameter]
        public EventCallback<MouseEventArgs> ClickEvent { get; set; }

        protected override string Css => $"btn {ButtonFlavor.Trim()} {ButtonColour.Trim()} {AddOnCss.Trim()}".Trim();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.Show)
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "type", this.ButtonType);
                builder.AddAttribute(2, "class", this.Css);
                builder.AddAttribute(3, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, this.ButtonClick));
                builder.AddContent(4, ChildContent);
                builder.CloseElement();
            }
        }

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

        }
        public static class Size
        {
            public static string BtnLarge = "btn-lg";
            public static string BtnSmall = "btn-sm";
        }

        #endregion
    }
}
