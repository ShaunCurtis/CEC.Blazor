using CEC.Blazor.Server.Services;
using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Routes
{
    public partial class Hello : ComponentBase, IDisposable
    {

        [Inject]
        protected CosmicDirectoryService CosmicDirectoryService { get; set; }

        protected string Message => CosmicDirectoryService?.Message ?? "Waiting...";

        protected override Task OnInitializedAsync()
        {
            this.CosmicDirectoryService.MessageChanged += this.MessageUpdated;
            return Task.CompletedTask;
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return Task.CompletedTask;
        }

        protected void MessageUpdated(object sender, EventArgs e) => InvokeAsync(this.StateHasChanged);

        public void Dispose() => this.CosmicDirectoryService.MessageChanged -= this.MessageUpdated;

        public void ButtonClicked(bool fast)
        {
            this.CosmicDirectoryService.GetWorldAsync(fast).ContinueWith(task =>
            {
                InvokeAsync(this.StateHasChanged);
            });
        }

        public async void UnsafeButtonClicked(bool fast)
        {
            await this.CosmicDirectoryService.BlackHoleWarning(true);
            var task = this.CosmicDirectoryService.BlackHoleWarning(false);
            var result = task.Result;
        }

        public Task TestUnsafe(bool fast)
        {
            Task.Run(() => this.CosmicDirectoryService.GetWorld(fast)).ContinueWith(task => {
                this.CosmicDirectoryService.GetWorldAsync(false).ContinueWith(task => {
                    InvokeAsync(this.StateHasChanged);
                });
            });
            return Task.CompletedTask;
        }
    }
}
