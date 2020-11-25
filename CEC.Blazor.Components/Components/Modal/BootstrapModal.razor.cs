using CEC.Blazor.Components;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using CEC.Blazor.Core;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// A container that shows Forms in a "pseudo" modal Dialog box format.
    /// The modal dialog is controlled by the _ShowModal property.
    /// The dialog is opened by calling Show passing in the form control to display.
    /// Show returns a task which is set to complete when either Dismiss or Close is called
    /// The component has a cascade to pass itself down to any child components.  These call Close to close the dialog
    /// Inbuild Options to control CSS:
    ///  -  ContainerCSS
    ///  -  ModalCSS
    ///  -  ModalHeaderCSS
    ///  -  ModalBodyCSS
    /// </summary>
    public partial class BootstrapModal : Component, IModal
    {
        /// <summary>
        /// Modal Options Property
        /// </summary>
        public ModalOptions Options { get; set; } = new ModalOptions();

        /// <summary>
        /// Render Fragment for the control content
        /// </summary>
        private RenderFragment _Content { get; set; }

        /// <summary>
        /// Property to track the modal state
        /// </summary>
        private bool _ShowModal { get; set; }

        /// <summary>
        /// Bootstrap CSS specific properties 
        /// </summary>

        private string _ContainerCss => $"modal fade show {this.Options.GetParameterAsString("ContainerCSS")}".Trim();

        private string _ModalCss => $"modal-dialog {this.Options.GetParameterAsString("ModalCSS")}".Trim();

        private string _ModalHeaderCss => $"modal-header {this.Options.GetParameterAsString("ModalHeaderCSS")}".Trim();

        private string _ModalBodyCss => $"modal-body {this.Options.GetParameterAsString("ModalBodyCSS")}".Trim();

        /// <summary>
        /// Independant Task passed to Show callers to track component state
        /// </summary>
        private TaskCompletionSource<ModalResult> _modalcompletiontask { get; set; } = new TaskCompletionSource<ModalResult>();

        /// <summary>
        /// Method called to show the component.  Returns a task which is set to complete when Dismiss or Close is called internally
        /// </summary>
        /// <typeparam name="TModal"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public Task<ModalResult> ShowAsync<TModal>(ModalOptions options) where TModal : IComponent
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
            InvokeAsync(Render);
            return _modalcompletiontask.Task;
        }

        /// <summary>
        /// Method to update the state of the display based on UIOptions
        /// </summary>
        /// <param name="options"></param>
        public void Update(ModalOptions options = null)
        {
            this.Options = options ??= this.Options;
            InvokeAsync(Render);
        }

        /// <summary>
        /// Method called by the dismiss button to close the dialog
        /// sets the task to complete, show to false and renders the component (which hides it as show is false!)
        /// </summary>
        public async void Dismiss()
        {
            _ = _modalcompletiontask.TrySetResult(ModalResult.Cancel());
            this._ShowModal = false;
            this._Content = null;
            await InvokeAsync(Render);
        }

        /// <summary>
        /// Method called by child components through the cascade value of this component
        /// sets the task to complete, show to false and renders the component (which hides it as show is false!)
        /// </summary>
        /// <param name="result"></param>
        public async void Close(ModalResult result)
        {
            _ = _modalcompletiontask.TrySetResult(result);
            this._ShowModal = false;
            this._Content = null;
            await InvokeAsync(Render);
        }
    }
}
