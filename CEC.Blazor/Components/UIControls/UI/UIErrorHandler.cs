using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.UIControls
{
/// <summary>
/// A UI component that only displays the Child Content when contwent loading is complete and there are no errors
/// This removes the need for complex error checking - such as if a record or list exists - in the child content.
/// Connect IsError and IsLoading to boolean properties in the parent and update them when loading is complete.
/// </summary>
    public class UIErrorHandler : UIBase
    {
        /// <summary>
        /// Boolean Property that determines if the child content or an error message is diplayed
        /// </summary>
        [Parameter]
        public bool IsError { get; set; } = false;

        /// <summary>
        /// Boolean Property that determines if the child content or an loading message is diplayed
        /// </summary>
        [Parameter]
        public bool IsLoading { get; set; } = true;

        /// <summary>
        /// CSS Override
        /// </summary>
        protected override string _BaseCss => this.IsLoading? "text-center p-3": "label label-error m-2";

        /// <summary>
        /// Customer error message to display
        /// </summary>
        [Parameter]
        public string ErrorMessage { get; set; } = "An error has occured loading the content";

        
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            this.ClearDuplicateAttributes();
            if (IsLoading)
            {
                builder.OpenElement(1, "div");
                builder.AddAttribute(2, "class", this._Css);
                builder.OpenElement(3, "button");
                builder.AddAttribute(4, "class", "btn btn-primary");
                builder.AddAttribute(5, "type", "button");
                builder.AddAttribute(6, "disabled", "disabled");
                builder.OpenElement(7, "span");
                builder.AddAttribute(8, "class", "spinner-border spinner-border-sm pr-2");
                builder.AddAttribute(9, "role", "status");
                builder.AddAttribute(10, "aria-hidden", "true");
                builder.CloseElement();
                builder.AddContent(11, "  Loading...");
                builder.CloseElement();
                builder.CloseElement();
            }
            else if (IsError)
            {
                builder.OpenElement(1, "div");
                builder.OpenElement(2, "span");
                builder.AddAttribute(3, "class", this._Css);
                builder.AddContent(4, ErrorMessage);
                builder.CloseElement();
                builder.CloseElement();
            }
            else builder.AddContent(1, ChildContent);
        }

    }
}
