using CEC.Blazor.Components.Base;
using CEC.Blazor.Data;
using CEC.Blazor.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardGrid<TRecord> where TRecord : IDbRecord<TRecord>, new()
    {

        [Parameter]
        public RenderFragment Title { get; set; }

        [Parameter]
        public RenderFragment TableHeader { get; set; }

        [Parameter]
        public RenderFragment<TRecord> RowTemplate { get; set; }

        [Parameter]
        public RenderFragment TableFooter { get; set; }

        [Parameter]
        public RenderFragment Navigation { get; set; }

        [Parameter]
        public RenderFragment Top { get; set; }

        [Parameter]
        public RenderFragment Footer { get; set; }

        [Parameter]
        public IControllerPagingService<TRecord> Paging { get; set; }

        protected string CardCSS { get; set; } = "card mt-2";

        protected string HeaderFontSize => this.IsMainHeader ? "display-5": "display-6";

        protected string CardHeaderCSS => "card-header bg-secondary text-white";

        protected string CardBodyCSS => this.Collapsed ? $"{this._CardBodyCSS} collapse" : $"{this._CardBodyCSS} collapse show";

        protected string CollapseButtonCSS { get; set; } = "btn btn-sm btn-outline-primary float-right p-2";

        [Parameter]
        public bool IsCollapsible { get; set; } = true;

        [Parameter]
        public bool IsMainHeader { get; set; } = false;

        [Parameter]
        public int Columns { get; set; } = 3;

        public bool IsPaging => this.Paging != null;

        public int MaxColumn => this.UIWrapper?.UIOptions?.MaxColumn ?? 2;

        private string _CardBodyCSS => "card body card-body-no-margin";

        protected bool Collapsed { get; set; } = false;
        
        protected string CollapseText { get => this.Collapsed ? "Show" : "Hide"; }

        private bool IsLoading => this.Paging == null || this.Paging.PagedRecords == null || this.Paging.PagedRecords.Count < 1;

        private bool IsError => !this.Paging?.HasPagedRecords ?? true ;

        protected void Toggle()
        {
            this.Collapsed = !this.Collapsed;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
        }
    }
}
