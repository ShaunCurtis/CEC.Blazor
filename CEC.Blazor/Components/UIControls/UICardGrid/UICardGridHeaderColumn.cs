using Microsoft.AspNetCore.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardGridHeaderColumn<TRecord> : UIGridHeaderColumn
    {
        [CascadingParameter]
        public PagingData<TRecord> Paging { get; set; }

        [Parameter]
        public string FieldName { get; set; }

        [Parameter]
        public string DisplayName { get; set; }

        protected string FieldDisplayName { get => string.IsNullOrEmpty(this.DisplayName) ? FieldName : DisplayName; }

        protected override string Css => $"{base.Css} column-sort";

        protected bool Sorted { get => !string.IsNullOrEmpty(this.FieldName); }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int i = 0;
            builder.OpenElement(i, "div");
            builder.AddAttribute(i++, "class", this.Css);
            if (this.Sorted && this.Paging != null)
            {
                if (!string.IsNullOrEmpty(this.ComponentId)) builder.AddAttribute(i++, "id", this.ComponentId);
                builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.Paging.Sort(e, this.FieldName))));
                builder.OpenElement(i, "span");
                builder.AddAttribute(i++, "class", this.Paging.GetIcon(this.FieldName));
                builder.CloseElement();
            }
            builder.AddContent(i++, this.FieldDisplayName);
            builder.CloseElement();
        }

    }
}
