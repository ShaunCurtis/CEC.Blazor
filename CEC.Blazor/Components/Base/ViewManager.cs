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
    class ViewManager : IComponent
    {
        private readonly RenderFragment _renderDelegate;
        private readonly RenderFragment _renderPageWithParametersDelegate;
        private RenderHandle _renderHandle;

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
 
        /// <summary>
        /// Gets or sets the type of a layout to be used if the page does not
        /// declare any layout. If specified, the type must implement <see cref="IComponent"/>
        /// and accept a parameter named <see cref="LayoutComponentBase.Body"/>.
        /// </summary>
        [Parameter]
        public Type DefaultLayout { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="RouteView"/>.
        /// </summary>
        public ViewManager()
        {
            // Cache the delegate instances
            _renderDelegate = Render;
            _renderPageWithParametersDelegate = RenderPageWithParameters;
        }

        /// <inheritdoc />
        public void Attach(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        /// <inheritdoc />
        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
            return this.LoadView();
        }

        /// <summary>
        /// Method tp load a new view
        /// </summary>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public Task LoadView(ViewData viewData = null)
        {
            this.ViewData =  viewData ?? this.ViewData;
            if (_ViewData == null)
            {
                throw new InvalidOperationException($"The {nameof(ViewManager)} component requires a non-null value for the parameter {nameof(ViewData)}.");
            }
            _renderHandle.Render(_renderDelegate);
            return Task.CompletedTask;

        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="builder">The <see cref="RenderTreeBuilder"/>.</param>
        protected virtual void Render(RenderTreeBuilder builder)
        {
            var pageLayoutType = _ViewData.PageType.GetCustomAttribute<LayoutAttribute>()?.LayoutType ?? DefaultLayout;

            builder.OpenComponent<CascadingValue<ViewManager>>(0);
            builder.AddAttribute(1, "Value", this);
            builder.OpenComponent<LayoutView>(1);
            builder.AddAttribute(2, nameof(LayoutView.Layout), pageLayoutType);
            builder.AddAttribute(3, nameof(LayoutView.ChildContent), _renderPageWithParametersDelegate);
            builder.CloseComponent();
            builder.CloseComponent();
        }

        private void RenderPageWithParameters(RenderTreeBuilder builder)
        {
            builder.OpenComponent(0, _ViewData.PageType);

            foreach (var kvp in _ViewData.ViewValues)
            {
                builder.AddAttribute(1, kvp.Key, kvp.Value);
            }
            builder.CloseComponent();
        }
    }
}
