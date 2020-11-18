using System;

namespace CEC.Weather.Extensions
{
    public static class DecimalExtensions
    {
        public static string AsLatitude(this decimal value)  => value > 0 ? $"{value}N" : $"{Math.Abs(value)}S";

        public static string AsLongitude(this decimal value) => value > 0 ? $"{value}E" : $"{Math.Abs(value)}W";
    }
}
