using CEC.Blazor.Server.Services;
using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Pages
{
    public partial class Index : ComponentBase, IDisposable
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
            if (firstRender) return this.CosmicDirectoryService.GetWorldAsync(true);
            else return Task.CompletedTask;
        }

        protected void MessageUpdated(object sender, EventArgs e) => InvokeAsync(this.StateHasChanged);

        public void Dispose() => this.CosmicDirectoryService.MessageChanged -= this.MessageUpdated;

        public void ButtonClicked(bool fast)
        {
            // This calls the unsafe function described in the Async Programming Article
            // this.CosmicDirectoryService.GetWorld(fast);
            this.CosmicDirectoryService.GetWorldAsync(fast).ContinueWith(task =>
            {
                InvokeAsync(this.StateHasChanged);
            });
        }

        public void UnsafeButtonClicked(bool fast)
        {
            // This calls the unsafe function described in the Async Programming Article
            this.CosmicDirectoryService.GetWorld(fast);
        }
    }
}
