#nullable disable warnings
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using CEC.Blazor.Components.UIControls;
using CEC.Blazor.Components.Modal;
using Microsoft.JSInterop;

namespace CEC.Blazor.Components.Base
{
    /// <summary>
    /// Displays the specified view component, rendering it inside its layout
    /// and any further nested layouts.
    /// </summary>
    public class ViewManager : IComponent
    {

        [Inject]
        private IJSRuntime _js { get; set; }

        /// <summary>
        /// Gets or sets the default view data.
        /// /// </summary>
        [Parameter]
        public ViewData DefaultViewData { get; set; }

        /// <summary>
        /// Gets or sets the view data.
        /// </summary>
        public ViewData ViewData
        {
            get
            {
                if (this._ViewData == null) this._ViewData = this.DefaultViewData;
                return this._ViewData;
            }
            protected set
            {
                this.LastViewData = this._ViewData;
                this._ViewData = value;
            }
        }

        /// <summary>
        /// ViewData used by the component
        /// </summary>
        private ViewData _ViewData { get; set; }

        /// <summary>
        /// Gets or sets the view data.
        /// </summary>
        public ViewData LastViewData { get; protected set; }

        /// <summary>
        /// The type of Modal Dialog to use for modals
        /// </summary>
        [Parameter] public Type ModalType { get; set; } = typeof(BootstrapModal);

        /// <summary>
        /// Property referencing the Bootstrap modal instance
        /// </summary>
        public IModal ModalDialog { get; protected set; }

        /// <summary>
        /// Property to lock the View - i.e. stop any new view loading
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// The Application Exit State - set to false if there's unsaved data in the applicaton
        /// </summary>
        private bool ExitState { get; set; }

        /// <summary>
        /// The Current Rendered View
        /// </summary>
        private Type CurrentView { get; set; }

        /// <summary>
        /// The RenderHandle to the Renderer
        /// </summary>
        private RenderHandle _renderHandle;

        /// <summary>
        /// Render Fragment to render this object
        /// </summary>
        private readonly RenderFragment _componentRenderFragment;

        /// <summary>
        /// Boolean Flag to track if there's a pending render event queued
        /// </summary>
        private bool _RenderEventQueued;

        /// <summary>
        /// Gets or sets the type of a layout to be used if the page does not
        /// declare any layout. If specified, the type must implement <see cref="IComponent"/>
        /// and accept a parameter named <see cref="LayoutComponentBase.Body"/>.
        /// </summary>
        [Parameter]
        public Type DefaultLayout { get; set; }

        /// <summary>
        /// Constructor - builds the component render fragment
        /// </summary>
        public ViewManager() => _componentRenderFragment = builder =>
        {
            this._RenderEventQueued = false;
            BuildRenderTree(builder);
        };

        /// <inheritdoc />
        public void Attach(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        /// <summary>
        /// Method to check if <param name="view"> is the current View
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public bool IsCurrentView(Type view) => this.CurrentView == view;

        public bool IsView => this._ViewData?.PageType != null;

        /// <inheritdoc />
        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
            this.ViewHasChanged();
            return Task.CompletedTask;
            //return this.LoadView();
        }

        /// <summary>
        /// Method to force a UI update
        /// Queues a render of the component
        /// </summary>
        public void ViewHasChanged()
        {
            if (!this._RenderEventQueued)
            {
                this._RenderEventQueued = true;
                _renderHandle.Render(_componentRenderFragment);
            }
        }

        /// <summary>
        /// Method tp load a new view
        /// </summary>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public Task LoadViewAsync(ViewData viewData = null)
        {
            if (!this.IsLocked)
            {
                if (viewData != null) this.ViewData = viewData;
                if (ViewData == null)
                {
                    throw new InvalidOperationException($"The {nameof(ViewManager)} component requires a non-null value for the parameter {nameof(ViewData)}.");
                }
                this.CurrentView = this._ViewData.PageType;
                this.ViewHasChanged();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method tp load a new view
        /// </summary>
        /// <param name="viewtype"></param>
        /// <returns></returns>
        public Task LoadViewAsync(Type viewtype)
        {
            var viewData = new ViewData(viewtype, new Dictionary<string, object>());
            return this.LoadViewAsync(viewData);
        }

        /// <summary>
        /// Method tp load a new view
        /// </summary>
        /// <typeparam name="TView"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task LoadViewAsync<TView>(Dictionary<string, object> data = null)
        {
            var viewData = new ViewData(typeof(TView), data);
            return this.LoadViewAsync(viewData);
        }

        /// <summary>
        /// Method tp load a new view
        /// </summary>
        /// <typeparam name="TView"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task LoadViewAsync<TView>(string key, object value)
        {
            var data = new Dictionary<string, object>();
            data.Add(key, value);
            var viewData = new ViewData(typeof(TView), data);
            return this.LoadViewAsync(viewData);
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="builder">The <see cref="RenderTreeBuilder"/>.</param>
        protected virtual void BuildRenderTree(RenderTreeBuilder builder)
        {
            // Adds cascadingvalue for the ViewManager
            builder.OpenComponent<CascadingValue<ViewManager>>(0);
            builder.AddAttribute(1, "Value", this);
            // Get the layout render fragment
            builder.AddAttribute(2, "ChildContent", this._layoutViewFragment);
            builder.CloseComponent();
        }

        /// <summary>
        /// Render fragment that Renders the LayoutView
        /// </summary>
        private RenderFragment _layoutViewFragment =>
            builder =>
            {
                // Adds the Modal Dialog infrastructure
                var pageLayoutType = ViewData?.PageType?.GetCustomAttribute<LayoutAttribute>()?.LayoutType ?? DefaultLayout;
                builder.OpenComponent(0, ModalType);
                builder.AddComponentReferenceCapture(1, modal => this.ModalDialog = (IModal)modal);
                builder.CloseComponent();
                // Adds the Layout component
                if (pageLayoutType != null)
                {
                    builder.OpenComponent<LayoutView>(2);
                    builder.AddAttribute(3, nameof(LayoutView.Layout), pageLayoutType);
                    // Adds the view render fragment into the layout component
                    if (this._ViewData != null)
                        builder.AddAttribute(4, nameof(LayoutView.ChildContent), this._viewFragment);
                    else
                    {
                        builder.AddContent(2, this._fallbackFragment);
                    }
                    builder.CloseComponent();
                }
                else
                {
                    builder.AddContent(0, this._fallbackFragment);
                }
            };

        /// <summary>
        /// Render fragment that renders the View
        /// </summary>
        private RenderFragment _viewFragment =>
            builder =>
            {
                // Adds the defined view with any defined parameters
                builder.OpenComponent(0, _ViewData.PageType);
                if (this._ViewData.ViewParameters != null)
                {
                    foreach (var kvp in _ViewData.ViewParameters)
                    {
                        builder.AddAttribute(1, kvp.Key, kvp.Value);
                    }
                }
                builder.CloseComponent();
            };

        /// <summary>
        /// Fallback render fragment if there's no Layout or View specified
        /// </summary>
        private RenderFragment _fallbackFragment =>
            builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "This is the ViewManager's fallback View.  You have no View and/or Layout specified.");
                builder.CloseElement();
            };

        /// <summary>
        /// Method to open a Modal Dialog
        /// </summary>
        /// <typeparam name="TForm"></typeparam>
        /// <param name="modalOptions"></param>
        /// <returns></returns>
        public async Task<ModalResult> ShowModalAsync<TForm>(ModalOptions modalOptions) where TForm : IComponent => await this.ModalDialog.Show<TForm>(modalOptions);

        /// <summary>
        /// Method to set or unset the browser onbeforeexit challenge
        /// true = ask question before exit
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public void SetPageExitCheck(bool action)
        {
            if (action != ExitState) _js.InvokeAsync<bool>("setExitCheck", action);
            ExitState = action;
        }
    }
}
