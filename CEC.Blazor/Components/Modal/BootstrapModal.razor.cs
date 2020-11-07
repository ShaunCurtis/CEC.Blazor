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

        private RenderFragment _Content { get; set; }

        private bool _ShowModal { get; set; }

        private Task modalResult { get; set; }

        private string _ContainerCss => $"modal fade show {this.Options.ContainerCSS}".Trim();

        private string _ModalCss => $"modal-dialog {this.Options.ModalCSS}";

        private string _ModalHeaderCss => $"modal-header {this.Options.ModalHeaderCSS}".Trim();

        private string _ModalBodyCss => $"modal-body {this.Options.ModalBodyCSS}".Trim();

        private TaskCompletionSource<BootstrapModalResult> _modalcompletiontask { get; set; } = new TaskCompletionSource<BootstrapModalResult>();

        public Task<BootstrapModalResult> Show<TModal>(BootstrapModalOptions options) where TModal: IComponent 
        {
            this.Options = options;
            this._modalcompletiontask = new TaskCompletionSource<BootstrapModalResult>();
            var i = 0;
            this._Content = new RenderFragment(builder =>
            {
                builder.OpenComponent(i++, typeof(TModal));
                builder.CloseComponent();
            });
            this._ShowModal = true;
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
            this._ShowModal = false;
            this._Content = null;
            await InvokeAsync(StateHasChanged);
        }

        public async void Close(BootstrapModalResult result)
        {
            _ = _modalcompletiontask.TrySetResult(result);
            this._ShowModal = false;
            this._Content = null;
            await InvokeAsync(StateHasChanged);
        }

    }
}
