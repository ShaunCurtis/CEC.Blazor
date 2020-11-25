using System.ComponentModel.DataAnnotations;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Base Data Record
    /// </summary>
    public class DbDistinct
    {
        [Key]
        public string Value { get; set; }

    }
}
