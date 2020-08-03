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
                int i = -1;
                builder.OpenElement(i++, "div");
                builder.AddMultipleAttributes(i++, AdditionalAttributes);
                builder.AddAttribute(i++, "class", this._Css);
                builder.OpenElement(i++, "div");
                builder.AddAttribute(i++, "class", this._HeaderCss);
                if (this.Header != null)
                {
                    builder.OpenElement(i++, "h4");
                    builder.AddContent(i++, this.Header);
                    builder.CloseElement();
                }
                if (this.IsCollapsible)
                {
                    builder.OpenElement(i++, "button");
                    builder.AddAttribute(i++, "class", this.CollapseButtonCSS);
                    builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.Toggle())));
                    builder.AddContent(i++, this.CollapseText);
                    builder.CloseElement();
                }
                builder.CloseElement();
                builder.OpenElement(i++, "div");
                builder.AddAttribute(i++, "class", this._BodyCss);
                if (this.Navigation != null) builder.AddContent(i++, this.Navigation);
                if (this.Body != null) builder.AddContent(i++, this.Body);
                if (this.Buttons != null) builder.AddContent(i++, this.Buttons);
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
