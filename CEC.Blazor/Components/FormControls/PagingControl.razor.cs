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

        private bool IsPaging => this.Paging != null;

        private bool IsPagination => this.Paging != null && this.Paging.IsPagination;

        protected override void OnInitialized()
        {
            // Check if we have a cascaded IControllerPagingService if so use it
            this.Paging = this._Paging?? this.Paging;
            base.OnInitialized();
        }
        protected override Task OnParametersSetAsync()
        {
            if (this.IsPaging)
            {
                this.Paging.PageHasChanged += this.UpdateUI;
                if (this.DisplayType == PagingDisplayType.Narrow) Paging.PagingBlockSize = 4;
                if (BlockSize > 0) Paging.PagingBlockSize = this.BlockSize;
            }
            return base.OnParametersSetAsync();
        }

        protected void UpdateUI(object sender, int recordno) => InvokeAsync(StateHasChanged);

        private string IsCurrent(int i) => i == this.Paging.CurrentPage ? "active" : string.Empty;
    }
}
