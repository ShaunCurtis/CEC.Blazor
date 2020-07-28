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
        protected WorldService WorldService { get; set; }

        [Inject]
        protected RouterSessionService RouterSessionService { get; set; }

        [Inject]
        protected ProtectedSessionStorage ProtectedSessionStorage { get; set; }


        public Guid GUID = Guid.NewGuid();

        protected string Message { get; set; }

        private string Alert = "Go!"; 

        protected override Task OnInitializedAsync()
        {
            this.WorldService.MessageChanged += this.MessageUpdated;
            this.Message = this.WorldService.Message;
            this.Message = "Waiting for an intercosmic connection";
            return Task.CompletedTask;
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender) await this.WorldService.GetWorld();
        }

        protected void MessageUpdated(object sender, EventArgs e)
        {
            this.Message = this.WorldService.Message;
            this.Alert = this.WorldService.Message;
            InvokeAsync(this.StateHasChanged);
        }

        public void Dispose()
        {
            this.WorldService.MessageChanged -= this.MessageUpdated;
        }

        public void buttonclick()
        {
            this.Message = "Here We Go";
            this.StateHasChanged();
            this.WorldService.GetWorld();
        }

        public void buttonchange()
        {
            this.Alert = this.WorldService.Message;
        }

    }
}
