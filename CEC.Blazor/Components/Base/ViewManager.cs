#nullable disable warnings
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using CEC.Blazor.Components.UIControls;
using CEC.Blazor.Components.Modal;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.WebUtilities;
using CEC.Blazor.Extensions;

namespace CEC.Blazor.Components.Base
{
    /// <summary>
    /// Displays the specified view component, rendering it inside its layout
    /// and any further nested layouts.
    /// </summary>
    public class ViewManager : IComponent
    {
        [Inject] private NavigationManager NavManager { get; set; }

        [Inject] private IJSRuntime _js { get; set; }

        /// <summary>
        /// Gets or sets the default view data.
        /// /// </summary>
        [Parameter] public ViewData DefaultViewData { get; set; }

        /// <summary>
        /// The type of Modal Dialog to use for modals
        /// </summary>
        [Parameter] public Type ModalType { get; set; } = typeof(BootstrapModal);

        /// <summary>
        /// Gets or sets the type of a layout to be used if the page does not
        /// declare any layout. If specified, the type must implement <see cref="IComponent"/>
        /// and accept a parameter named <see cref="LayoutComponentBase.Body"/>.
        /// </summary>
        [Parameter] public Type DefaultLayout { get; set; }

        /// <summary>
        /// The size of the History list.
        /// </summary>
        [Parameter] public int ViewHistorySize { get; set; } = 10;

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
                this.AddViewToHistory(this._ViewData);
                this._ViewData = value;
            }
        }

        /// <summary>
        /// Property that stores the View History.  It's size is controlled by ViewHistorySize
        /// </summary>
        public SortedList<DateTime, ViewData> ViewHistory { get; private set; } = new SortedList<DateTime, ViewData>();

        /// <summary>
        /// ViewData used by the component
        /// </summary>
        private ViewData _ViewData { get; set; }

        /// <summary>
        /// Gets or sets the view data.
        /// </summary>
        public ViewData LastViewData
        {
            get
            {
                var newest = ViewHistory.Max(item => item.Key);
                if (newest != null) return ViewHistory[newest];
                else return null;
            }
        }
        /// <summary>
        /// Property referencing the Bootstrap modal instance
        /// </summary>
        public IModal ModalDialog { get; protected set; }

        /// <summary>
        /// Property to lock the View - i.e. stop any new view loading
        /// </summary>
        public bool IsLocked { get; private set; }

        /// <summary>
        /// The Application Exit State - set to false if there's unsaved data in the applicaton
        /// </summary>
        private bool ExitState { get; set; }

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
        public bool IsCurrentView(Type view) => this.ViewData?.PageType == view;

        public bool IsView => this._ViewData?.PageType != null;

        /// <inheritdoc />
        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
            this._ViewData = this.DefaultViewData;
            this.ReadViewDataFromQueryString();
            this.Render();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method to force a UI update
        /// Queues a render of the component
        /// </summary>
        private void Render() => InvokeAsync(() =>
        {
            if (!this._RenderEventQueued)
            {
                this._RenderEventQueued = true;
                _renderHandle.Render(_componentRenderFragment);
            }
        }
        );

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
                this.Render();
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
                try
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
                }
                catch
                {
                    // If the pagetype causes an error - load the fallback
                    builder.AddContent(0, this._fallbackFragment);
                }
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
            if (action != ExitState) _js.InvokeAsync<bool>("cecblazor_setEditorExitCheck", action);
            ExitState = action;
        }

        /// <summary>
        /// Method to read ViewData information from the Uri querystring
        /// Probably needs some more work on data types and parsing
        /// </summary>
        private void ReadViewDataFromQueryString()
        {
            ViewData viewData = null;
            var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
            var vals = QueryHelpers.ParseQuery(uri.Query);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("View", out var classname))
            {
                var type = this.FindType(classname);
                if (type != null)
                {
                    viewData = new ViewData(type, null);
                    foreach (var set in vals)
                    {
                        if (set.Key.StartsWith("Param-"))
                        {
                            object value;
                            if (DateTime.TryParse(set.Value, out DateTime datevalue)) value = datevalue;
                            else if (Int32.TryParse(set.Value, out int intvalue)) value = intvalue;
                            else if (Decimal.TryParse(set.Value, out decimal decvalue)) value = decvalue;
                            else value = set.Value;
                            viewData.SetParameter(set.Key.Replace("Param-", ""), value);
                        }
                        if (set.Key.StartsWith("Field-"))
                        {
                            object value;
                            if (DateTime.TryParse(set.Value, out DateTime datevalue)) value = datevalue;
                            else if (Int32.TryParse(set.Value, out int intvalue)) value = intvalue;
                            else if (Decimal.TryParse(set.Value, out decimal decvalue)) value = decvalue;
                            else value = set.Value;
                            viewData.SetField(set.Key.Replace("Field-", ""), value);
                        }
                    }
                }
            }
            if (viewData != null) this.ViewData = viewData;
        }

        /// <summary>
        ///  Method to find a Type in the AppDomain Assemblies
        /// </summary>
        /// <param name="qualifiedTypeName"></param>
        /// <returns></returns>
        private Type FindType(string qualifiedTypeName)
        {
            Type t = Type.GetType(qualifiedTypeName);

            if (t != null) return t;
            else
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = asm.GetType(qualifiedTypeName);
                    if (t != null) return t;
                }
                return null;
            }
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
        /// Method to add a View to the View History and manage it's size
        /// </summary>
        /// <param name="value"></param>
        private void AddViewToHistory(ViewData value)
        {
            while (this.ViewHistory.Count >= this.ViewHistorySize)
            {
                var oldest = ViewHistory.Min(item => item.Key);
                this.ViewHistory.Remove(oldest);
            }
            this.ViewHistory.Add(DateTime.Now, value);
        }

        /// <summary>
        /// Method to lock the View
        /// </summary>
        public void LockView()
        {
            this.IsLocked = true;
            this.SetPageExitCheck(true);
        }

        /// <summary>
        /// Method to unlock the View
        /// </summary>
        public void UnLockView()
        {
            this.IsLocked = false;
            this.SetPageExitCheck(false);
        }

    }
}
