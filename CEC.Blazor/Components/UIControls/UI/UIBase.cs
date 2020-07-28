using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// Base UI Rendering Wrapper to build a Botstrap Component
    /// </summary>

    public class UIBase : ComponentBase
    {

        [Parameter]
        public string ComponentId { get; set; } = "";

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string Css { get; set; } = string.Empty;

        [Parameter]
        public string AddOnCss { get; set; } = string.Empty;

        [Parameter]
        public bool IsFormGroup { get; set; }

        [CascadingParameter]
        public UIOptions UIOptions { get; set; } = new UIOptions();

        protected virtual string _Tag { get; set; } = "div";

        protected string FormGroup => this.IsFormGroup ? "form-group " : string.Empty;

        protected virtual string _Css => $"{this.Css} {AddOnCss.Trim()}".Trim();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int i = 0;
            builder.OpenElement(i, this._Tag);
            builder.AddAttribute(i++, "class", this._Css);
            if (!string.IsNullOrEmpty(this.ComponentId)) builder.AddAttribute(i++, "id", this.ComponentId);
            if (this.ChildContent != null) builder.AddContent(i++, ChildContent);
            builder.CloseElement();
        }
    }
}
