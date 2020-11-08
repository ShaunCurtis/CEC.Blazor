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
        public Dictionary<string, object> ViewValues { get; private set; }

        /// <summary>
        /// Constructs an instance of <see cref="ViewData"/>.
        /// </summary>
        /// <param name="viewType">The type of the view, which must implement <see cref="IView"/>.</param>
        /// <param name="viewValues">The view parameter values.</param>
        public ViewData(Type pageType, Dictionary<string, object> viewValues)
        {
            if (pageType == null) throw new ArgumentNullException(nameof(pageType));
            if (!typeof(IView).IsAssignableFrom(pageType)) throw new ArgumentException($"The view must implement {nameof(IView)}.", nameof(pageType));
            this.PageType = pageType;
            this.ViewValues = viewValues;
        }

        public bool GetValue(string key, out object value)
        {
            value = null;
            if (this.ViewValues.ContainsKey(key)) value = this.ViewValues[key];
            return this.ViewValues.ContainsKey(key);
        }

        public object GetValue(string key)
        {
            if (this.ViewValues.ContainsKey(key)) return this.ViewValues[key];
            else return null;
        }

        public string GetValueAsString(string key)
        {
            if (this.ViewValues.ContainsKey(key) && this.ViewValues[key] is string) return (string)this.ViewValues[key];
            else return string.Empty;
        }

        public void SetValue(string key, object value)
        {
            if (this.ViewValues.ContainsKey(key)) this.ViewValues[key] = value;
            else this.ViewValues.Add(key, value);
        }


    }
}
