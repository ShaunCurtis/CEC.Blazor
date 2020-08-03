using CEC.Blazor.Components.Base;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardGrid<TItem> : UICardListBase
    {

        [Parameter]
        public RenderFragment TableHeader { get; set; }

        [Parameter]
        public RenderFragment<TItem> RowTemplate { get; set; }

        [Parameter]
        public RenderFragment TableFooter { get; set; }

        [Parameter]
        public RenderFragment Navigation { get; set; }

        [Parameter]
        public RenderFragment Top { get; set; }

        [Parameter]
        public RenderFragment Footer { get; set; }

        [Parameter]
        public string ListTitle { get; set; } = "Collaspible Card";

        [Parameter]
        public PagingData<TItem> Paging { get; set; }

        public string CardCSS { get; set; } = "card mt-2";

        public string HeaderFontSize => this.IsMainHeader ? "display-5": "display-6";

        public string CardHeaderCSS => "card-header bg-secondary text-white";

        public string CardBodyCSS => this.Collapsed ? $"{this._CardBodyCSS} collapse" : $"{this._CardBodyCSS} collapse show";

        public string CardCollapseButtonCSS { get; set; } = "float-right";

        [Parameter]
        public bool IsCollapsible { get; set; } = true;

        [Parameter]
        public bool IsMainHeader { get; set; } = false;

        [Parameter]
        public int Columns { get; set; } = 3;

        public bool IsPaging => this.Paging != null;

        private string _CardBodyCSS => "card body card-body-no-margin";

        protected bool Collapsed { get; set; } = false;
        
        protected string CollapseText { get => this.Collapsed ? "Show" : "Hide"; }

        private bool IsLoading => this.Paging.Records == null || this.Paging.Records.Count < 1;

        private bool IsError { get; set; }

        protected void Toggle()
        {
            this.Collapsed = !this.Collapsed;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (this.IsLoading && firstRender) this.IsError = true;
        }
    }
}
