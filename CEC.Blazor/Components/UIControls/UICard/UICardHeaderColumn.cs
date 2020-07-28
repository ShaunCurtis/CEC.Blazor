using Microsoft.AspNetCore.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System.Runtime.CompilerServices;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardHeaderColumn<TRecord> : UITColumn<TRecord>
    {

        [Parameter]
        public string FieldName { get; set; }

        [Parameter]
        public string DisplayName { get; set; }

        public bool Show { get; set; } = true;

        protected override string Tag => "th";

        protected string FieldDisplayName { get => string.IsNullOrEmpty(this.DisplayName) ? FieldName : DisplayName; }

        protected string Style => this.IsMaxColumn ? $"width: {this.UIOptions.MaxColumnPercent}%" : string.Empty;

        protected bool IsMaxColumn => this.UIOptions != null && this.UIOptions.MaxColumn == this.Column;

        protected string OverflowCss => this.IsMaxColumn ? " td-overflow" : string.Empty;

        protected string MaxColumnCss => this.IsMaxColumn ? " td-max" : string.Empty;

        protected override string _Css => this.Sorted ? $"{base._Css}{this.OverflowCss}{this.MaxColumnCss} column-sort cursor-hand" : $"{base._Css}{this.OverflowCss}{this.MaxColumnCss}";

        protected bool Sorted { get => (this.Card?.IsPaging ?? false) && !string.IsNullOrEmpty(this.FieldName); }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.Show)
            {
                int i = 0;
                builder.OpenElement(i, this.Tag);
                builder.AddAttribute(i++, "class", this._Css);
                if (!string.IsNullOrEmpty(this.Style)) builder.AddAttribute(i++, "style", this.Style);
                builder.AddAttribute(i++, "scope", "col");
                if (!string.IsNullOrEmpty(this.ComponentId)) builder.AddAttribute(i++, "id", this.ComponentId);
                if (this.Sorted)
                {
                    builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.Card.Paging.Sort(e, this.FieldName))));
                    builder.OpenElement(i, "span");
                    builder.AddAttribute(i++, "class", this.Card.Paging?.GetIcon(this.FieldName) ?? "");
                    builder.CloseElement();
                }
                builder.AddContent(i++, this.FieldDisplayName);
                builder.CloseElement();
            }
        }
    }
}
