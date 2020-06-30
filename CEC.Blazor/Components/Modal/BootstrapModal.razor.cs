using CEC.Blazor.Components.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.UIControls
{
    public partial class BootstrapModal : ComponentBase
    {
        public BootstrapModalOptions Options { get; set; } = new BootstrapModalOptions();

        public RenderFragment Content { get; set; }

        public bool ShowModal { get; set; }

        private Task modalResult { get; set; }

        private TaskCompletionSource<BootstrapModalResult> _modalcompletiontask { get; set; } = new TaskCompletionSource<BootstrapModalResult>();

        protected override void OnInitialized()
        {
        }

        public Task<BootstrapModalResult> Show<TModal>(BootstrapModalOptions options)
        {
            this.Options = options;
            this._modalcompletiontask = new TaskCompletionSource<BootstrapModalResult>();
            var i = 0;
            this.Content = new RenderFragment(builder =>
            {
                builder.OpenComponent(i++, typeof(TModal));
                builder.CloseComponent();
            });
            this.ShowModal = true;
            InvokeAsync(StateHasChanged);
            return _modalcompletiontask.Task;
        }

        public void Update(BootstrapModalOptions options = null)
        {
            this.Options = options??= this.Options;
            InvokeAsync(StateHasChanged);
        }

        public async void Dismiss()
        {
            _ = _modalcompletiontask.TrySetResult(BootstrapModalResult.Cancel());
            this.ShowModal = false;
            await InvokeAsync(StateHasChanged);
        }

        public async void Close(BootstrapModalResult result)
        {
            _ = _modalcompletiontask.TrySetResult(result);
            this.ShowModal = false;
            await InvokeAsync(StateHasChanged);
        }

    }
}
