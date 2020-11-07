#nullable disable warnings
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Components.Base
{
    /// <summary>
    /// Displays the specified view component, rendering it inside its layout
    /// and any further nested layouts.
    /// </summary>
    public class ViewManager : IComponent
    {
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
        /// Gets or sets the default view data.
        /// /// </summary>
        [Parameter]
        public ViewData DefaultViewData { get; set; }

        /// <summary>
        /// Gets or sets the view data.
        /// </summary>
        public ViewData ViewData { get; set; }

        /// <summary>
        /// ViewData used by the component
        /// </summary>
        private ViewData _ViewData => this.ViewData ?? this.DefaultViewData;

        private Type CurrentView { get; set; }

        /// <summary>
        /// Gets or sets the type of a layout to be used if the page does not
        /// declare any layout. If specified, the type must implement <see cref="IComponent"/>
        /// and accept a parameter named <see cref="LayoutComponentBase.Body"/>.
        /// </summary>
        [Parameter]
        public Type DefaultLayout { get; set; }

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
        /// Method to check if view is the current View
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public bool IsCurrentView(Type view) => this.CurrentView == view;

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
        public Task LoadView(ViewData viewData = null)
        {
            this.ViewData = viewData ?? this._ViewData;
            if (_ViewData == null)
            {
                throw new InvalidOperationException($"The {nameof(ViewManager)} component requires a non-null value for the parameter {nameof(ViewData)}.");
            }
            this.CurrentView = viewData.PageType;
            this.ViewHasChanged();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="builder">The <see cref="RenderTreeBuilder"/>.</param>
        protected virtual void BuildRenderTree(RenderTreeBuilder builder)
        {
            var pageLayoutType = _ViewData.PageType.GetCustomAttribute<LayoutAttribute>()?.LayoutType ?? DefaultLayout;

            builder.OpenComponent<CascadingValue<ViewManager>>(0);
            builder.AddAttribute(1, "Value", this);
            builder.AddAttribute(2, "ChildContent", this._layoutViewFragment);
            builder.CloseComponent();
        }
        private RenderFragment _layoutViewFragment =>
            builder =>
            {
                builder.OpenComponent<LayoutView>(0);
                builder.AddAttribute(1, nameof(LayoutView.Layout), DefaultLayout);
                builder.AddAttribute(2, nameof(LayoutView.ChildContent), this._viewFragment);
                builder.CloseComponent();
            };

        private RenderFragment _testFragement =>
            builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "This is a component");
                builder.CloseElement();
            };

        private RenderFragment _viewFragment =>
            builder =>
            {
                builder.OpenComponent(0, _ViewData.PageType);
                if (this._ViewData.ViewValues != null)
                {
                    foreach (var kvp in _ViewData.ViewValues)
                    {
                        builder.AddAttribute(1, kvp.Key, kvp.Value);
                    }
                }
                builder.CloseComponent();
            };
    }
}
