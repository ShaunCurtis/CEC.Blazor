using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEC.Weather.Data
{
    /// <summary>
    /// Data Record for a Weather Foreecast
    /// Data validation is handled by the Fluent Validator
    /// </summary>
    public class DbWeatherForecast :IDbRecord<DbWeatherForecast>
    {
        public int WeatherForecastID { get; set; } = -1;

        public DateTime Date { get; set; } = DateTime.Now.Date;

        //[Required]
        //[Range(-40, 60, ErrorMessage = "Only Temperatures between -40 an 60 are allowed.")]
        public decimal TemperatureC { get; set; } = 20;

        public bool Frost { get; set; }

        [NotMapped]
        public WeatherSummary Summary { get; set; } = WeatherSummary.Unknown;

        [NotMapped]
        public WeatherOutlook Outlook { get; set; } = WeatherOutlook.Sunny;

        public string Description { get; set; } = string.Empty;

        //[RegularExpression(@"^([A-PR-UWYZ0-9][A-HK-Y0-9][AEHMNPRTVXY0-9]?[ABEHMNPRVWXY0-9]? {1,2}[0-9][ABD-HJLN-UW-Z]{2}|GIR 0AA)$", ErrorMessage = "This must be a valid UK Post Code - try GL2 5TP")]
        public string PostCode { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;

        [NotMapped]
        public decimal TemperatureF => decimal.Round(32 + (TemperatureC / 0.5556M), 2);

        public int OutlookValue
        {
            get => (int)this.Outlook;
            set => this.Outlook = (WeatherOutlook)value;
        }

        public int SummaryValue
        {
            get => (int)this.Summary;
            set => this.Summary = (WeatherSummary)value;
        }

        [NotMapped]
        public int ID => this.WeatherForecastID;

        [NotMapped]
        public string DisplayName => $"Forecast for {this.Date.AsShortDate()}";

        public DbWeatherForecast ShadowCopy()
        {
            return new DbWeatherForecast() {
                Date = this.Date,
                TemperatureC = this.TemperatureC,
                Frost = this.Frost,
                Outlook = this.Outlook,
                Description = this.Description,
                Summary = this.Summary,
                Detail = this.Detail
            };
        }
    }
}
