
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace CEC.Blazor.Extensions
{
    public static class NavigationExtensions
    {
/// <summary>
/// Helper to get query strings from the Navigation Manager
/// Thanks to Chris Sainty for code
/// http://chrissainty.com
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="navManager"></param>
/// <param name="key"></param>
/// <param name="value"></param>
/// <returns></returns>
        public static bool TryGetQueryString<T>(this NavigationManager navManager, string key, out T value)
        {
            var uri = navManager.ToAbsoluteUri(navManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var valueFromQueryString))
            {
                if (typeof(T) == typeof(int) && int.TryParse(valueFromQueryString, out var valueAsInt))
                {
                    value = (T)(object)valueAsInt;
                    return true;
                }

                if (typeof(T) == typeof(string))
                {
                    value = (T)(object)valueFromQueryString.ToString();
                    return true;
                }

                if (typeof(T) == typeof(decimal) && decimal.TryParse(valueFromQueryString, out var valueAsDecimal))
                {
                    value = (T)(object)valueAsDecimal;
                    return true;
                }
            }

            value = default;
            return false;
        }

    }
}
