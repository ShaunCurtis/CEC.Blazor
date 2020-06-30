using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using CEC.Blazor.Data;

namespace CEC.Blazor.Components
{
    public partial class UIPagingControl<TRecord> : ComponentBase
    {
        [CascadingParameter]
        public PagingData<TRecord> _Paging { get; set; }

        [Parameter]
        public PagingData<TRecord> Paging { get; set; }

        [Parameter]
        public PagingDisplayType DisplayType { get; set; } = PagingDisplayType.Full;

        [Parameter]
        public int BlockSize { get; set; } = 0;

        private bool IsPagination { get => this.Paging != null && this.Paging.IsPagination; }

        protected override void OnInitialized()
        {
            if (this._Paging != null) this.Paging = this._Paging;
            base.OnInitialized();
            if (this.Paging != null) this.Paging.PageHasChanged += this.UpdateUI;
        }
        protected override Task OnParametersSetAsync()
        {
            if (this.DisplayType == PagingDisplayType.Narrow) Paging.BlockSize = 4;
            if (BlockSize > 0) Paging.BlockSize = this.BlockSize;
            return base.OnParametersSetAsync();
        }

        protected void UpdateUI(object sender, int recordno) { this.StateHasChanged(); }

        private string IsCurrent(int i)
        {
            if (i == this.Paging.CurrentPage) return "active";
            return string.Empty;
        }
    }
}
