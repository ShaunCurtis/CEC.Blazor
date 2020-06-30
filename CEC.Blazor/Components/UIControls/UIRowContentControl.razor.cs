using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a row or row fragment from a property label and value
    ///  Provides a structured  mechanism for managing Bootstrap class elements used in Editors and Viewers in one place. 
    /// The properties are pretty self explanatory and therefore not decorated with summaries
    /// </summary>
    public partial class UIRowContentControl : ComponentBase
    {

        [Parameter]
        public string Label { get; set; } = string.Empty;

        [Parameter]
        public int LabelSize { get; set; } = -1;

        [Parameter]
        public int ControlSize { get; set; } = 0;

        [Parameter]
        public bool isRow { get; set; } = true;

        [Parameter]
        public bool isRowOnly { get; set; } = false;

        [Parameter]
        public bool isModal { get; set; } = false;

        [Parameter]
        public bool isRequired { get; set; } = false;

        [Parameter]
        public bool RightAligned { get; set; } = false;

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public RenderFragment LabelControl { get; set; }

        [Parameter]
        public string CustomLabelCss { get; set; } = string.Empty;

        [Parameter]
        public string CustomControlCss { get; set; } = string.Empty;

        [Parameter]
        public string RowCss { get; set; } = "form-group row";

        protected string ControlAlignment => this.RightAligned ? "text-right" : string.Empty;

        protected string LabelCss => string.Concat("col-", this._LabelSize, " ", this.CustomLabelCss);

        protected string ControlCss => string.Concat("col-", this._ControlSize, " ", this.CustomControlCss, " ", this.ControlAlignment);

        protected string RequiredCss => isRequired ? "text-danger" : "";

        protected bool IsLabel => !string.IsNullOrEmpty(this.Label);

        protected bool ShowLabel => this._LabelSize > 0; 

        protected string LabelText => this.Label.Contains(":") ? this.Label : string.Concat(this.Label, ":");

        protected int _LabelSize => this.LabelSize == -1 ? this.isModal ? 3 : 2 : this.LabelSize;

        protected int _ControlSize => this.ControlSize == 0 ? this.isModal ? 9 : 10 : this.ControlSize;
    }
}
