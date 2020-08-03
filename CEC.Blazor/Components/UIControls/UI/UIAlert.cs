using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Components.UIControls
{
    public class UIAlert : UIBase
    {
        /// <summary>
        /// Alert to display
        /// </summary>
        [Parameter]
        public Alert Alert { get; set; } = new Alert();

        /// <summary>
        /// Property for sizing alert
        /// </summary>
        [Parameter]
        public bool IsSmall { get; set; }

        /// <summary>
        /// Boolean Show override
        /// </summary>
        protected override bool _Show => this.Alert?.IsAlert ?? false;

        /// <summary>
        /// CSS override
        /// </summary>
        protected override string _Css => this.CleanUpCss(this.IsSmall ? $"alert alert-sm {this.Alert.CSS}" : $"alert {this.Alert.CSS}");

        /// <summary>
        /// Override the content with the alert message
        /// </summary>
        protected override string _Content => this.Alert?.Message ?? string.Empty;

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
