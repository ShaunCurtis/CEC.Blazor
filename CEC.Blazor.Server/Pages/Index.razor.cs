using CEC.Blazor.Server.Services;
using Microsoft.AspNetCore.Components;
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

        public Guid GUID = Guid.NewGuid();

        protected string Message { get; set; }

        private string Alert = "Go!"; 

        protected async override Task OnInitializedAsync()
        {
            this.WorldService.MessageChanged += this.MessageUpdated;
            this.Message = this.WorldService.Message;
            await this.WorldService.GetWorld();
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
