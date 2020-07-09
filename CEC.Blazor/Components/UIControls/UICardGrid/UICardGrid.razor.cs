using CEC.Blazor.Components.Base;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardGrid<TItem> : UICardGridBase
    {

        [Parameter]
        public RenderFragment GridHeader { get; set; }

        [Parameter]
        public RenderFragment<TItem> GridTemplate { get; set; }

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

        protected bool Collapsed { get; set; } = false;
        
        protected string CollapseCSS { get => this.Collapsed ? "collapse" : "collapse show"; }
        
        protected string CollapseText { get => this.Collapsed ? "Show" : "Hide"; }

        protected void Toggle()
        {
            this.Collapsed = !this.Collapsed;
        }
    }
}
