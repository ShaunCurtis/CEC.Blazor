using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.Base
{
    public abstract class Component : IComponent, IHandleEvent, IHandleAfterRender
    {
        private readonly RenderFragment _renderFragment;
        private RenderHandle _renderHandle;
        private bool _firstRender = true;
        private bool _hasNeverRendered = true;
        private bool _hasPendingQueuedRender;
        private bool _hasCalledOnAfterRender;

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
            if (_hasPendingQueuedRender)
            {
                return;
            }

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
        /// Returns a flag to indicate whether the component should render.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ShouldRender() => true;


        protected virtual Task OnAfterRenderAsync(bool firstRender)
            => Task.CompletedTask;

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
        void IComponent.Attach(RenderHandle renderHandle)
        {
            if (_renderHandle.IsInitialized)
            {
                throw new InvalidOperationException($"The render handle is already set. Cannot initialize a {nameof(ComponentBase)} more than once.");
            }
            _firstRender = true;
            _renderHandle = renderHandle;
        }

        public virtual async Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
            await this._StartRenderAsync();
        }

        public virtual async Task ResetAsync()
        {
            this._firstRender = true;
            await this._StartRenderAsync();
        }

        private async Task _StartRenderAsync()
        {
            await this.OnRenderAsync(this._firstRender);
            this._firstRender = false; 
            this.Render();
        }

        public virtual Task OnRenderAsync(bool firstRender) => Task.CompletedTask;


        Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object arg)
        {
            var task = callback.InvokeAsync(arg);
            var shouldAwaitTask = task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Canceled;
            this.Render();
            return shouldAwaitTask ? CallRenderOnAsyncCompletion(task) : Task.CompletedTask;
        }

        Task IHandleAfterRender.OnAfterRenderAsync()
        {
            var firstRender = !_hasCalledOnAfterRender;
            _hasCalledOnAfterRender |= true;
            return OnAfterRenderAsync(firstRender);
        }

        private async Task CallRenderOnAsyncCompletion(Task task)
        {
            try
            {
                await task;
            }
            catch // avoiding exception filters for AOT runtime support
            {
                // Ignore exceptions from task cancellations, but don't bother issuing a state change.
                if (task.IsCanceled) return;
                else throw;
            }
            Render();
        }
    }
}
