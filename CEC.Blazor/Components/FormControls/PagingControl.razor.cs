using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using CEC.Blazor.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Components.Base;

namespace CEC.Blazor.Components
{
    public partial class PagingControl<TRecord> : ControlBase where TRecord : IDbRecord<TRecord>, new()
    {
        [CascadingParameter] public IControllerPagingService<TRecord> _Paging { get; set; }

        [Parameter] public IControllerPagingService<TRecord> Paging { get; set; }

        [Parameter] public PagingDisplayType DisplayType { get; set; } = PagingDisplayType.Full;

        [Parameter] public int BlockSize { get; set; } = 0;

        private bool IsPaging => this.Paging != null;

        private bool IsPagination => this.Paging != null && this.Paging.IsPagination;

        protected override Task OnRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Check if we have a cascaded IControllerPagingService if so use it
                this.Paging = this._Paging ?? this.Paging;
            }
            if (this.IsPaging)
            {
                this.Paging.PageHasChanged += this.UpdateUI;
                if (this.DisplayType == PagingDisplayType.Narrow) Paging.PagingBlockSize = 4;
                if (BlockSize > 0) Paging.PagingBlockSize = this.BlockSize;
            }
            return base.OnRenderAsync(firstRender);
        }

        protected async void UpdateUI(object sender, int recordno) => await RenderAsync();

        private string IsCurrent(int i) => i == this.Paging.CurrentPage ? "active" : string.Empty;
    }
}
