using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace CEC.Blazor.Core
{
    /// <summary>
    /// Describes information for a view
    /// </summary>
    public sealed class ViewData
    {
        /// <summary>
        /// Gets the type of the page matching the route.
        /// </summary>
        public Type PageType { get; set;}

        /// <summary>
        /// Parameter values to add to the Route when created
        /// </summary>
        public Dictionary<string, object> ViewParameters { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// View values that can be used by the view and subcomponents
        /// </summary>
        public Dictionary<string, object> ViewFields { get; private set; } = new Dictionary<string, object>();

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
            if (viewValues != null) this.ViewParameters = viewValues;
        }

        public bool GetParameter(string key, out object value)
        {
            value = null;
            if (this.ViewParameters.ContainsKey(key)) value = this.ViewParameters[key];
            return this.ViewParameters.ContainsKey(key);
        }

        public object GetParameter(string key)
        {
            if (this.ViewParameters.ContainsKey(key)) return this.ViewParameters[key];
            else return null;
        }

        public bool GetParameterAsString(string key, out string value)
        {
                value = string.Empty;
                var val = GetParameter(key);
                if (val is string)
                {
                    value = (string)this.ViewFields[key];
                    return true;
                }
                return false;
        }

        public void SetParameter(string key, object value)
        {
            if (this.ViewParameters.ContainsKey(key)) this.ViewParameters[key] = value;
            else this.ViewParameters.Add(key, value);
        }

        public bool GetField(string key, out object value)
        {
            value = null;
            if (this.ViewFields.ContainsKey(key)) value = this.ViewFields[key];
            return this.ViewFields.ContainsKey(key);
        }

        public object GetField(string key)
        {
            if (this.ViewFields.ContainsKey(key)) return this.ViewFields[key];
            else return null;
        }

        public bool GetFieldAsString(string key, out string value)
        {
            value = string.Empty;
            var val = GetField(key);
            if (val is string)
            {
                value = (string)this.ViewFields[key];
                return true;
            }
            return false;
        }

        public bool GetFieldAsInt(string key, out int value)
        {
            value = 0;
            var val = GetField(key);
            if (val is int)
            {
                value = (int)this.ViewFields[key];
                return true;
            }
            return false;
        }

        public void SetField(string key, object value)
        {
            if (this.ViewFields.ContainsKey(key)) this.ViewFields[key] = value;
            else this.ViewFields.Add(key, value);
        }


    }
}
