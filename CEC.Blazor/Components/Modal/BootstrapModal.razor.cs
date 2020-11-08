using CEC.Blazor.Components.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.UIControls
{
    public partial class BootstrapModal : ComponentBase, IModal
    {
        public ModalOptions Options { get; set; } = new ModalOptions();

        private RenderFragment _Content { get; set; }

        private bool _ShowModal { get; set; }

        private Task modalResult { get; set; }

        private string _ContainerCss => $"modal fade show {this.Options.GetParameterAsString("ContainerCSS")}".Trim();

        private string _ModalCss => $"modal-dialog {this.Options.GetParameterAsString("ModalCSS")}".Trim();

        private string _ModalHeaderCss => $"modal-header {this.Options.GetParameterAsString("ModalHeaderCSS")}".Trim();

        private string _ModalBodyCss => $"modal-body {this.Options.GetParameterAsString("ModalBodyCSS")}".Trim();

        private TaskCompletionSource<ModalResult> _modalcompletiontask { get; set; } = new TaskCompletionSource<ModalResult>();

        public Task<ModalResult> Show<TModal>(ModalOptions options) where TModal: IComponent 
        {
            this.Options = options;
            this._modalcompletiontask = new TaskCompletionSource<ModalResult>();
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

        public void Update(ModalOptions options = null)
        {
            this.Options = options??= this.Options;
            InvokeAsync(StateHasChanged);
        }

        public async void Dismiss()
        {
            _ = _modalcompletiontask.TrySetResult(ModalResult.Cancel());
            this._ShowModal = false;
            this._Content = null;
            await InvokeAsync(StateHasChanged);
        }

        public async void Close(ModalResult result)
        {
            _ = _modalcompletiontask.TrySetResult(result);
            this._ShowModal = false;
            this._Content = null;
            await InvokeAsync(StateHasChanged);
        }
    }
}
