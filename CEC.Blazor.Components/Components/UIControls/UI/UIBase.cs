using CEC.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// Base UI Rendering Wrapper to build a Css Framework Html Component
    /// This is a base component implementing ControlBase - not a ComponentBase inherited class.
    /// Note that many of the parameter properties have a protected _property
    /// You can override the value used by setting the _property specifically in any derived classes
    /// The _property is the property actually used in the render process.
    /// e.g. protected override string _Tag => "div";
    /// will force the component tag to be a div. 
    /// </summary>

    public abstract class UIBase : ControlBase
    {

        #region Public Properties

        /// <summary>
        /// Gets or sets a collection of additional attributes that will be applied to the created <c>form</c> element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> AdditionalAttributes { get; set; }

        /// <summary>
        /// Child Content to add to Component
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Css for component - can be overridden and fixed in inherited components
        /// </summary>
        [Parameter]
        public virtual string Tag { get; set; } = "div";

        /// <summary>
        /// Css for component - can be overridden and fixed in inherited components
        /// </summary>
        [Parameter]
        public virtual string Css { get; set; } = string.Empty;

        /// <summary>
        /// Additional Css to tag on the end of the base Css
        /// </summary>
        [Parameter]
        public string AddOnCss { get; set; } = string.Empty;

        /// <summary>
        /// Boolean property that dictates if the componet is rendered
        /// </summary>
        [Parameter]
        public virtual bool Show { get; set; } = true;

        #endregion

        #region Protected properties used by inheriting components

        /// <summary>
        /// Html attributes that need to be removed if set on the control
        /// default is only the class attribute
        /// </summary>
        protected List<string> UsedAttributes { get; set; } = new List<string>() { "class" };


        /// <summary>
        /// Html tag for the control - default is a div.
        /// In general use css display to change the block behaviour
        /// </summary>
        protected virtual string _Tag => this.Tag;

        /// <summary>
        /// Property for fixing the base Css.  Base returns the Parameter Css, but can be overridden in inherited classes
        /// </summary>
        protected virtual string _BaseCss => this.Css;

        /// <summary>
        /// Property for fixing the Add On Css.  Base returns the Parameter AddOnCss, but can be overridden say to String.Empty in inherited classes
        /// </summary>
        protected virtual string _AddOnCss => this.AddOnCss;

        /// <summary>
        /// Actual calculated Css string used in the component
        /// </summary>
        protected virtual string _Css => this.CleanUpCss($"{this._BaseCss} {this._AddOnCss}");

        /// <summary>
        /// Show Property used by the builder - allows override of the Parameter set version
        /// </summary>
        protected virtual bool _Show => this.Show;

        /// <summary>
        /// Property to override the content of the component
        /// </summary>
        protected virtual string _Content => string.Empty;

        #endregion

        #region Methods

        /// <summary>
        /// inherited
        /// </summary>
        /// <param name="builder"></param>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this._Show)
            {
                this.ClearDuplicateAttributes();
                builder.OpenElement(0, this._Tag);
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                builder.AddAttribute(2, "class", this._Css);
                if (!string.IsNullOrEmpty(this._Content)) builder.AddContent(3, (MarkupString)this._Content);
                else if (this.ChildContent != null) builder.AddContent(4, ChildContent);
                builder.CloseElement();
            }
        }

        /// <summary>
        /// Method to clean up the Additional Attributes
        /// </summary>
        protected void ClearDuplicateAttributes()
        {
            if (this.AdditionalAttributes != null && this.UsedAttributes != null)
            {
                foreach (var item in this.UsedAttributes)
                {
                    if (this.AdditionalAttributes.ContainsKey(item)) this.AdditionalAttributes.Remove(item);
                }
            }
        }

        /// <summary>
        /// Method to clean up the Css - remove leading and trailing spaces and any multiple spaces
        /// </summary>
        /// <param name="css"></param>
        /// <returns></returns>
        protected string CleanUpCss(string css)
        {
            while (css.Contains("  ")) css = css.Replace("  ", " ");
            return css.Trim();
        }

        #endregion
    }
}
