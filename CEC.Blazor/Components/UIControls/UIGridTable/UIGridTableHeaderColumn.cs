
using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Grid Column
    /// </summary>

    public class UIGridTableHeaderColumn<TRecord> : UIGridTableColumn<TRecord> where TRecord : IDbRecord<TRecord>, new()
    {

        [Parameter]
        public string FieldName { get; set; }

        protected override string _Css
        {
            get
            {
                var css = this.CleanUpCss($"grid-col {this.CssAlignment} {this.CssHeader} {this.MaxRowCss} {this.AddOnCss}");
                return this.Sorted ? $"{css}  column-sort cursor-hand" : css;
            }
        }

        protected bool Sorted { get => (this.Card?.IsPaging ?? false) && !string.IsNullOrEmpty(this.FieldName); }

        protected override void OnInitialized()
        {
            this.IsHeader = true;
            base.OnInitialized();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.Show)
            {
                int i = -1;
                builder.OpenElement(i++, this._Tag);
                builder.AddAttribute(i++, "class", this._Css);
                if (!string.IsNullOrEmpty(this.Style)) builder.AddAttribute(i++, "style", this.Style);
                if (this.ColumnSpan > 1) builder.AddAttribute(i++, "colspan", this.ColumnSpan);
                builder.AddAttribute(i++, "scope", "col");
                if (this.IsMaxColumn)
                {
                    builder.OpenElement(i++, "div");
                    builder.AddAttribute(i++, "class", "grid-overflow");
                    builder.OpenElement(i++, "div");
                    builder.AddAttribute(i++, "class", "grid-overflowinner");
                    this.AddSorting(ref builder,ref i);
                    this.AddContent(ref builder, ref i);
                    builder.CloseElement();
                    builder.CloseElement();
                }
                else
                {
                    this.AddSorting(ref builder, ref i);
                    this.AddContent(ref builder, ref i);
                }
                builder.CloseElement();
            }
        }

        private void AddSorting(ref RenderTreeBuilder builder, ref int i)
        {
            if (this.Sorted)
            {
                builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.Card.Paging.Sort(e, this.FieldName))));
                builder.OpenElement(i++, "span");
                builder.AddAttribute(i++, "class", this.Card.Paging?.GetIcon(this.FieldName) ?? "");
                builder.CloseElement();
            }

        }

        private void AddContent(ref RenderTreeBuilder builder, ref int i)
        {
            if (this.ChildContent != null) builder.AddContent(i++, this.ChildContent);
            else builder.AddContent(i++, this.FieldName);
        }

    }
}
