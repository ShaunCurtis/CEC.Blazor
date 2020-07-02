using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
using System;
using System.Reflection.Metadata.Ecma335;

namespace CEC.Blazor.Server.Data
{
    public class WeatherForecast :IDbRecord<WeatherForecast>
    {
        public int WeatherForecastID { get; set; }

        public DateTime Date { get; set; } = DateTime.Now.Date;

        public int TemperatureC { get; set; } = 20;

        public bool Frost { get; set; }

        public WeatherSummary Summary { get; set; } = WeatherSummary.Unknown;

        public WeatherOutlook Outlook { get; set; } = WeatherOutlook.Sunny;

        public string Description { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public int OutlookValue
        {
            get => (int)this.Outlook;
            set => this.Outlook = (WeatherOutlook)value;
        }

        public int ID => this.WeatherForecastID;

        public string DisplayName => $"Forecast for {this.Date.AsShortDate()}";

        public WeatherForecast ShadowCopy()
        {
            return new WeatherForecast() {
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
