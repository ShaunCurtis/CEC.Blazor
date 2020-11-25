using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Core
{
    /// <summary>
    /// Abstract Base Class implementing basic IComponent functions
    /// A lot of the code is common with ComponentBase
    /// Blazor Team copyright notices recognised.
    /// </summary>
    public abstract class Component : IComponent, IHandleEvent, IHandleAfterRender
    {
        private readonly RenderFragment _renderFragment;
        private RenderHandle _renderHandle;
        private bool _firstRender = true;
        private bool _hasNeverRendered = true;
        private bool _hasPendingQueuedRender;
        private bool _hasCalledOnAfterRender;

        /// <summary>
        /// Property to check if the component is loading -  set internally
        /// </summary>
        public bool Loading { get; protected set; } = true;

        /// <summary>
        /// Constructs an instance of <see cref="ControlBase"/>.
        /// </summary>
        public Component()
        {
            _renderFragment = builder =>
            {
                _hasPendingQueuedRender = false;
                _hasNeverRendered = false;
                BuildRenderTree(builder);
            };
        }

        /// <summary>
        /// Renders the component to the supplied <see cref="RenderTreeBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        protected virtual void BuildRenderTree(RenderTreeBuilder builder)
        {
            // Developers can either override this method in derived classes, or can use Razor
            // syntax to define a derived class and have the compiler generate the method.

            // Other code within this class should *not* invoke BuildRenderTree directly,
            // but instead should invoke the _renderFragment field.
        }


        /// <summary>
        /// Notifies the component that things have changed and it needs queue a render event on the Render.
        /// </summary>
        protected void Render()
        {
            if (_hasPendingQueuedRender) return;
            if (_hasNeverRendered || ShouldRender())
            {
                _hasPendingQueuedRender = true;
                try
                {
                    _renderHandle.Render(_renderFragment);
                }
                catch
                {
                    _hasPendingQueuedRender = false;
                    throw;
                }
            }
        }

        /// <summary>
        /// Method Called on a Component when it is attached to the RenderTree
        /// Only called once
        /// </summary>
        /// <returns></returns>
        protected Task OnAttachAsync() => Task.CompletedTask;

        /// <summary>
        /// Runs Render on the UI Thread
        /// </summary>
        /// <returns></returns>
        protected async Task RenderAsync() => await this.InvokeAsync(Render);

        /// <summary>
        /// Returns a flag to indicate whether the component should render.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ShouldRender() => true;

        /// <summary>
        /// Public Method called after the component has been rendered
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected virtual Task OnAfterRenderAsync(bool firstRender)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes the supplied work item on the associated renderer's
        /// synchronization context.
        /// </summary>
        /// <param name="workItem">The work item to execute.</param>
        protected Task InvokeAsync(Action workItem) => _renderHandle.Dispatcher.InvokeAsync(workItem);

        /// <summary>
        /// Executes the supplied work item on the associated renderer's
        /// synchronization context.
        /// </summary>
        /// <param name="workItem">The work item to execute.</param>
        protected Task InvokeAsync(Func<Task> workItem) => _renderHandle.Dispatcher.InvokeAsync(workItem);

        /// <summary>
        /// IComponent Attach implementation
        /// </summary>
        /// <param name="renderHandle"></param>
        async void IComponent.Attach(RenderHandle renderHandle)
        {
            if (_renderHandle.IsInitialized)
            {
                throw new InvalidOperationException($"The render handle is already set. Cannot initialize a {nameof(ComponentBase)} more than once.");
            }
            _firstRender = true;
            _renderHandle = renderHandle;
            await OnAttachAsync();
        }
        

        /// <summary>
        /// Implementation of the IComponent SetParametersAsync
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
            await this._StartRenderAsync();
        }

        /// <summary>
        /// Method to reset the component and render from scatch
        /// </summary>
        /// <returns></returns>
        public virtual async Task ResetAsync()
        {
            this._firstRender = true;
            this.Loading = true;
            await this._StartRenderAsync();
        }

        /// <summary>
        /// Internal Method called from SetParametersAsync to begin the render process
        /// Tracks if this is the first or subsequnet renders
        /// </summary>
        /// <returns></returns>
        private async Task _StartRenderAsync()
        {
            this.Loading = true;
            await RenderAsync();
            await this.OnRenderAsync(this._firstRender);
            this._firstRender = false;
            this.Loading = false;
            await RenderAsync();
        }

        /// <summary>
        /// Principle exposed Render "Event"
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected virtual Task OnRenderAsync(bool firstRender) => Task.CompletedTask;

        /// <summary>
        /// Internal method to track the render event
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object arg)
        {
            var task = callback.InvokeAsync(arg);
            var shouldAwaitTask = task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Canceled;
            InvokeAsync(Render);
            return shouldAwaitTask ? CallRenderOnAsyncCompletion(task) : Task.CompletedTask;
        }

        /// <summary>
        /// Internal Method triggered after the component has rendered calling OnAfterRenderAsync
        /// </summary>
        /// <returns></returns>
        Task IHandleAfterRender.OnAfterRenderAsync()
        {
            var firstRender = !_hasCalledOnAfterRender;
            _hasCalledOnAfterRender |= true;
            return OnAfterRenderAsync(firstRender);
        }

        /// <summary>
        /// Internal method to handle render completion
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private async Task CallRenderOnAsyncCompletion(Task task)
        {
            try { await task; }
            catch
            {
                if (task.IsCanceled) return;
                else throw;
            }
            await InvokeAsync(Render);
        }
    }
}
