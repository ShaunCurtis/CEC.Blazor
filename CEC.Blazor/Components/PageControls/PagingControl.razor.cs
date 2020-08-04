using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using CEC.Blazor.Data;
using CEC.Blazor.Services;

namespace CEC.Blazor.Components
{
    public partial class PagingControl<TRecord> : ComponentBase where TRecord : IDbRecord<TRecord>, new()
    {
        [CascadingParameter]
        public IControllerPagingService<TRecord> _Paging { get; set; }

        [Parameter]
        public IControllerPagingService<TRecord> Paging { get; set; }

        [Parameter]
        public PagingDisplayType DisplayType { get; set; } = PagingDisplayType.Full;

        [Parameter]
        public int BlockSize { get; set; } = 0;

        private bool IsPagination => this.Paging != null && this.Paging.IsPagination;

        protected override void OnInitialized()
        {
            if (this._Paging != null) this.Paging = this._Paging;
            base.OnInitialized();
            if (this.Paging != null) this.Paging.PageHasChanged += this.UpdateUI;
        }
        protected override Task OnParametersSetAsync()
        {
            if (this.DisplayType == PagingDisplayType.Narrow) Paging.PagingBlockSize = 4;
            if (BlockSize > 0) Paging.PagingBlockSize = this.BlockSize;
            return base.OnParametersSetAsync();
        }

        protected void UpdateUI(object sender, int recordno) => this.StateHasChanged();

        private string IsCurrent(int i) => i == this.Paging.CurrentPage ? "active" : string.Empty;
    }
}
