using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace CEC.Weather.Data
{
    /// <summary>
    /// Data Record for a Weather Forecast
    /// </summary>
    public record WeatherForecastRecord 
    {

        public int ID { get; init; } = -1;

        public DateTime Date { get; init; } = DateTime.Now.Date;

        public decimal TemperatureC { get; init; } = 20;

        public string PostCode { get; init; } = string.Empty;

        public bool Frost { get; init; }

        public int SummaryValue { get; init; } = 0;

        public int OutlookValue { get; init; } = 0;

        public string Description { get; init; } = string.Empty;

        public string Detail { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        [NotMapped]
        public int WeatherForecastID { get => this.ID; }

        [NotMapped]
        public WeatherSummary Summary => (WeatherSummary)this.SummaryValue;

        [NotMapped]
        public WeatherOutlook Outlook => (WeatherOutlook)this.OutlookValue;

        public DbWeatherForecast ShadowCopy()
        {
            return new DbWeatherForecast() {
                ID = this.ID,
                Date = this.Date,
                TemperatureC = this.TemperatureC,
                Frost = this.Frost,
                OutlookValue = this.OutlookValue,
                Description = this.Description,
                SummaryValue = this.SummaryValue,
                Detail = this.Detail,
                DisplayName = this.DisplayName
            };
        }
    }
}
