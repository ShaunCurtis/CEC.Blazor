
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Grid Column
    /// </summary>

    public class UIGridTableEditColumn<TRecord> : UIComponent
    {
        /// <summary>
        /// Cascaded UIGridTable
        /// </summary>
        [CascadingParameter]
        public UICardGrid<TRecord> Card { get; set; }

        /// <summary>
        /// Record ID passed via a cascade
        /// </summary>
        [CascadingParameter(Name = "RecordID")]
        public int RecordID { get; set; } = 0;

        /// <summary>
        /// HTML Colspan
        /// </summary>
        [Parameter]
        public int ColumnSpan { get; set; }

        /// <summary>
        /// Inherited
        /// </summary>
        protected override string _Css => this.CleanUpCss($"grid-col text-right {this.AddOnCss}");

        /// <summary>
        /// Button CSS
        /// </summary>
        private string _BadgeCss => "badge badge-primary p-2 text-white cursor-hand";

        /// <summary>
        /// Overridden Show
        /// </summary>
        protected override bool _Show => this.RecordID > 0 && this.Card != null;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this._Show)
            {
                int i = -1;
                builder.OpenElement(i++, this._Tag);
                builder.AddAttribute(i++, "class", this._Css);
                if (this.ColumnSpan > 1) builder.AddAttribute(i++, "colspan", this.ColumnSpan);
                builder.OpenElement(i++, "a");
                builder.AddAttribute(i++, "class", this._BadgeCss);
                builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.Card.NavigateToEditor(this.RecordID))));
                builder.AddContent(i++, "Edit");
                builder.CloseElement();
                builder.CloseElement();
            }
        }
    }
}
