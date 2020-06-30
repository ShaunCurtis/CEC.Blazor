using System;
using System.Globalization;

namespace CEC.Blazor.Extensions
{
    public static class DateExtensions
    {
        public static string AsShortDate(this DateTime value) => value.ToString("dd-MMM-yyyy");

        public static string AsShortDateTime(this DateTime value) => value.ToString("h:mm tt dd-MMM-yyyy");

        public static string AsMonthYear(this DateTime value) => value.ToString("MMM-yyyy");

        public static string AsMonthName(this int value) => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(value);

    }
}
