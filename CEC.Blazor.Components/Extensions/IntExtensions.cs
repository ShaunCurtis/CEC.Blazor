
namespace CEC.Blazor.Extensions
{
    public static class IntExtensions
    {

        public static string CheckforNull(this int value) => value == int.MinValue ? "No Value" : value.ToString();

        public static int AsLong(this int? value) => value is null ? 0 : (int)value;

    }
}
