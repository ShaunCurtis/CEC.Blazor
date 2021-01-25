using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.SPA.Components.UIControls
{
    public class UIAlert : UIBootstrapBase
    {
        /// <summary>
        /// Alert to display
        /// </summary>
        [Parameter]
        public Alert Alert { get; set; } = new Alert();

        /// <summary>
        /// Set the CssName
        /// </summary>
        protected override string CssName => "alert";

        /// <summary>
        /// Property to override the colour CSS
        /// </summary>
        protected override string ColourCssFragment => this.Alert != null ? GetCssFragment<Bootstrap.ColourCode>(this.Alert.ColourCode) : GetCssFragment<Bootstrap.ColourCode>(this.ColourCode);

        /// <summary>
        /// Boolean Show override
        /// </summary>
        protected override bool _Show => this.Alert?.IsAlert ?? false;

        /// <summary>
        /// Override the content with the alert message
        /// </summary>
        protected override string _Content => this.Alert?.Message ?? string.Empty;
    }
}
