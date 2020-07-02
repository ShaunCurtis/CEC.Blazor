using CEC.Blazor.Components.Base;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardList<TItem> : ApplicationComponentBase
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
        public IReadOnlyList<TItem> Items { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string ListTitle { get; set; } = "Collaspible Card";

        public string CardCSS { get; set; } = "mt-2";

        public string HeaderFontSize => this.IsMainHeader ? "display-5": "display-6";

        public string CardHeaderCSS => "bg-secondary text-white";

        public string CardBodyCSS { get; set; } = "card-body-no-margin";

        public string CardButtonCSS { get; set; } = "btn-sm btn-outline-light";

        [Parameter]
        public bool IsCollapsible { get; set; } = true;

        [Parameter]
        public bool IsMainHeader { get; set; } = false;

        [Parameter]
        public int Columns { get; set; } = 3;

        protected bool Collapsed { get; set; } = false;
        
        protected string CollapseCSS { get => this.Collapsed ? "collapse" : "collapse show"; }
        
        protected string CollapseText { get => this.Collapsed ? "Show" : "Hide"; }

        protected override void OnInitialized()
        {
        }
 
        protected void Toggle()
        {
            this.Collapsed = !this.Collapsed;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
