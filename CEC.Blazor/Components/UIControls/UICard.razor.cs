using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Server.Components.UIControls
{
    public partial class UICard : ComponentBase
    {
        /// <summary>
        ///  Property for the text to display in the header
        /// </summary>
        [Parameter]
        public RenderFragment Header { get; set; }

        /// <summary>
        /// Property to set the card as Dirty (unsaved
        /// Used by UI to change the card color)
        /// </summary>
        [Parameter]
        public bool Dirty { get; set; } = false;

        [Parameter]
        public string MoreCardCss { get; set; }

        [Parameter]
        public string MoreHeaderCss { get; set; }


        [Parameter]
        public string MoreBodyCss { get; set; }

        /// <summary>
        /// CSS for the toggle button
        /// </summary>
        [Parameter]
        public string CardButtonCSS { get; set; } = "btn-sm btn-outline-primary";

        [Parameter]
        public bool IsCollapsible { get; set; } = false;

        /// <summary>
        /// Property to set if card is/is not collapsed
        /// </summary>
        protected bool Collapsed { get; set; } = false;

        /// <summary>
        /// CSS for the collapsible body - toggled by the Collapsed Property flag
        /// </summary>
        protected string CollapseCSS { get => this.Collapsed ? "collapse" : "collapse show"; }

        /// <summary>
        /// Text to apper in the Collapse button - toggles on Collapsed Property Flag
        /// </summary>
        protected string CollapseText { get => this.Collapsed ? "Show" : "Hide"; }

        /// <summary>
        /// UI Property for Card CSS
        /// </summary>
        protected string CardCss { get => Dirty ? "card border-danger" : "card border-secondary"; }

        /// <summary>
        /// UI Property for Card Header CSS
        /// </summary>
        protected string CardHeaderCss { get => Dirty ? "card-header text-white bg-danger" : "card-header text-white bg-secondary"; }

        /// <summary>
        ///  Property for the main body in the control
        /// </summary>
        [Parameter]
        public RenderFragment Body { get; set; }

        /// <summary>
        ///  Property for the main body in the control
        /// </summary>
        [Parameter]
        public RenderFragment Navigation { get; set; }


        /// <summary>
        ///  Property for the main body in the control
        /// </summary>
        [Parameter]
        public RenderFragment Buttons { get; set; }

        /// <summary>
        /// Toggles the Collapsed Property if the Toggle button clicked
        /// this then changes the Css for the header
        /// </summary>
        protected void Toggle() => this.Collapsed = !this.Collapsed;

    }
}
