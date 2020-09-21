
using System.Diagnostics.CodeAnalysis;

namespace CEC.Blazor.Extensions
{
    public static class DecimalExtensions
    {

        public static string DecimalPlaces(this decimal value, int places ) => value.ToString($"N{places}");

    }
}
