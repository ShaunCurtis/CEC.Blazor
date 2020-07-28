using Microsoft.AspNetCore.Components;
using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    public class UIAlert : UIBase
    {
        [Parameter]
        public Alert Alert { get; set; } = new Alert();

        [Parameter]
        public bool IsSmall { get; set; }

        protected bool IsAlert => this.Alert != null && this.Alert.IsAlert;

        protected override string _Css => this.IsSmall ? $"alert alert-sm {this.Alert.CSS}" : $"alert {this.Alert.CSS}";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.IsAlert)
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", this._Css);
                builder.AddContent(2, (MarkupString)this.Alert.Message);
                builder.CloseElement();
            }
        }

        #region CSS Constants

        public static class Colour
        {
            public static string AlertPrimary = "alert-primary";
            public static string AlertSecondary = "alert-secondary";
            public static string AlertDanger = "alert-danger";
            public static string AlertWarning = "alert-warning";
            public static string AlertSuccess = "alert-success";
            public static string AlertDark = "alert-dark";
            public static string AlertLight = "alert-light";
            public static string AlertInfo = "alert-info";
        }

        #endregion
    }
}
