using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace CEC.Blazor.Components.Base
{
    /// <summary>
    /// Describes information for a view
    /// </summary>
    public sealed class ViewData
    {
        /// <summary>
        /// Gets the type of the page matching the route.
        /// </summary>
        public Type PageType { get; }

        /// <summary>
        /// Gets route parameter values extracted from the matched route.
        /// </summary>
        public IReadOnlyDictionary<string, object> ViewValues { get; }

        /// <summary>
        /// Constructs an instance of <see cref="ViewData"/>.
        /// </summary>
        /// <param name="viewType">The type of the view, which must implement <see cref="IView"/>.</param>
        /// <param name="viewValues">The view parameter values.</param>
        public ViewData(Type pageType, IReadOnlyDictionary<string, object> viewValues)
        {
            if (pageType == null)
            {
                throw new ArgumentNullException(nameof(pageType));
            }

            if (!typeof(IView).IsAssignableFrom(pageType))
            {
                throw new ArgumentException($"The view must implement {nameof(IView)}.", nameof(pageType));
            }

            PageType = pageType;
            //ViewValues = viewValues ?? throw new ArgumentNullException(nameof(viewValues));
        }
    }
}
