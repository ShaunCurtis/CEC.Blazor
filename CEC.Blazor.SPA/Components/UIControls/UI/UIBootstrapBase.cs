using Microsoft.AspNetCore.Components;
using System;

namespace CEC.Blazor.SPA.Components.UIControls
{
    public class UIBootstrapBase : UIBase
    {
        protected virtual string CssName { get; set; } = string.Empty;

        /// <summary>
        /// Bootstrap Colour for the Component
        /// </summary>
        [Parameter]
        public Bootstrap.ColourCode ColourCode { get; set; } = Bootstrap.ColourCode.info;

        /// <summary>
        /// Bootstrap Size for the Component
        /// </summary>
        [Parameter]
        public Bootstrap.SizeCode SizeCode { get; set; } = Bootstrap.SizeCode.normal;

        /// <summary>
        /// Property to set the HTML value if appropriate
        /// </summary>
        [Parameter]
        public string Value { get; set; } = "";

        /// <summary>
        /// Property to get the Colour CSS
        /// </summary>
        protected virtual string ColourCssFragment => GetCssFragment<Bootstrap.ColourCode>(this.ColourCode);

        /// <summary>
        /// Property to get the Size CSS
        /// </summary>
        protected virtual string SizeCssFragment => GetCssFragment<Bootstrap.SizeCode>(this.SizeCode);

        /// <summary>
        /// CSS override
        /// </summary>
        protected override string _Css => this.CleanUpCss($"{this.CssName} {this.SizeCssFragment} {this.ColourCssFragment} {this.AddOnCss}");

        /// <summary>
        /// Method to format as Bootstrap CSS Fragment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        protected string GetCssFragment<T>(T code) => $"{this.CssName}-{Enum.GetName(typeof(T), code).Replace("_", "-")}";

    }
}
