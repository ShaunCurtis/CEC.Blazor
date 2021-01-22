using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICard : UIBase
    {
        #region Content

        /// <summary>
        ///  Property for the text to display in the header
        /// </summary>
        [Parameter]
        public RenderFragment Header { get; set; }

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

        #endregion

        /// <summary>
        /// Property to set the card as Dirty (unsaved
        /// Used by UI to change the card color)
        /// </summary>
        [Parameter]
        public bool Dirty { get; set; } = false;

        /// <summary>
        /// Property to set if card is accordian style
        /// </summary>
        [Parameter]
        public bool IsCollapsible { get; set; } = false;

        #region Protected CSS Properties

        /// <summary>
        /// CSS for the toggle button
        /// </summary>
        protected string CollapseButtonCSS { get; set; } = "btn btn-sm btn-outline-primary float-right p-2";

        /// <summary>
        /// CSS for the collapsible body - toggled by the Collapsed Property flag
        /// </summary>
        protected string CollapseCSS { get => this.Collapsed ? "collapse" : "collapse show"; }

        /// <summary>
        /// UI Property for Card CSS
        /// </summary>
        protected override string _BaseCss { get => Dirty ? $"card border-danger" : $"card border-secondary"; }

        /// <summary>
        /// UI Property for Card Header CSS
        /// </summary>
        protected string _HeaderCss { get => Dirty ? "card-header text-white bg-danger" : "card-header text-white bg-secondary"; }

        /// <summary>
        /// UI Property for Card Body CSS
        /// </summary>
        protected string _BodyCss => "card-body";

        #endregion

        /// <summary>
        /// Property to set if card is/is not collapsed
        /// </summary>
        private bool Collapsed { get; set; } = false;

        /// <summary>
        /// Text to apper in the Collapse button - toggles on Collapsed Property Flag
        /// </summary>
        private string CollapseText { get => this.Collapsed ? "Show" : "Hide"; }

        /// <summary>
        /// inherited
        /// </summary>
        /// <param name="builder"></param>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.Show)
            {
                this.ClearDuplicateAttributes();
                builder.OpenElement(0, "div");
                builder.AddMultipleAttributes(2, AdditionalAttributes);
                builder.AddAttribute(2, "class", this._Css);
                builder.OpenElement(3, "div");
                builder.AddAttribute(4, "class", this._HeaderCss);
                if (this.Header != null)
                {
                    builder.OpenElement(5, "h4");
                    builder.AddContent(6, this.Header);
                    builder.CloseElement();
                }
                if (this.IsCollapsible)
                {
                    builder.OpenElement(7, "button");
                    builder.AddAttribute(8, "class", this.CollapseButtonCSS);
                    builder.AddAttribute(9, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.Toggle())));
                    builder.AddContent(10, this.CollapseText);
                    builder.CloseElement();
                }
                builder.CloseElement();
                builder.OpenElement(11, "div");
                builder.AddAttribute(12, "class", this._BodyCss);
                if (this.Navigation != null) builder.AddContent(13, this.Navigation);
                if (this.Body != null) builder.AddContent(14, this.Body);
                if (this.Buttons != null) builder.AddContent(15, this.Buttons);
                builder.CloseElement();
                builder.CloseElement();
            }
        }

        /// <summary>
        /// Toggles the Collapsed Property if the Toggle button clicked
        /// this then changes the Css for the header
        /// </summary>
        protected void Toggle() => this.Collapsed = !this.Collapsed;

    }
}
