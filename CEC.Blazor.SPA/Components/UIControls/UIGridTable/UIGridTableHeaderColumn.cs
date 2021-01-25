using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;

namespace CEC.Blazor.SPA.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Grid Column
    /// </summary>
    public class UIGridTableHeaderColumn<TRecord> : 
        UIGridTableColumn<TRecord>
          where TRecord : IDbRecord<TRecord>, new()
    {

        [Parameter]
        public string FieldName { get; set; }

        protected override string _Tag => "th";

        protected override string _Css
        {
            get
            {
                var css = this.CleanUpCss($"grid-col {this.CssAlignment} {this.CssHeader} {this.MaxRowCss} {this.AddOnCss}");
                return this.Sorted ? $"{css}  column-sort cursor-hand" : css;
            }
        }

        protected bool Sorted { get => (this.Card?.IsPaging ?? false) && !string.IsNullOrEmpty(this.FieldName); }

        public UIGridTableHeaderColumn()
        {
            this.IsHeader = true;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.Show)
            {
                builder.OpenElement(0, this._Tag);
                builder.AddAttribute(1, "class", this._Css);
                if (!string.IsNullOrEmpty(this.Style)) builder.AddAttribute(2, "style", this.Style);
                if (this.ColumnSpan > 1) builder.AddAttribute(3, "colspan", this.ColumnSpan);
                builder.AddAttribute(4, "scope", "col");
                if (this.IsMaxColumn)
                {
                    builder.OpenElement(5, "div");
                    builder.AddAttribute(6, "class", "grid-overflow");
                    builder.OpenElement(7, "div");
                    builder.AddAttribute(8, "class", "grid-overflowinner");
                    this.AddSorting(ref builder);
                    this.AddContent(ref builder);
                    builder.CloseElement();
                    builder.CloseElement();
                }
                else
                {
                    this.AddSorting(ref builder);
                    this.AddContent(ref builder);
                }
                builder.CloseElement();
            }
        }

        private void AddSorting(ref RenderTreeBuilder builder)
        {
            if (this.Sorted)
            {
                builder.AddAttribute(10, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.Card.Paging.Sort(e, this.FieldName))));
                builder.OpenElement(11, "span");
                builder.AddAttribute(12, "class", this.Card.Paging?.GetIcon(this.FieldName) ?? "");
                builder.CloseElement();
            }

        }

        private void AddContent(ref RenderTreeBuilder builder)
        {
            if (this.ChildContent != null) builder.AddContent(20, this.ChildContent);
            else builder.AddContent(21, this.FieldName);
        }

    }
}
