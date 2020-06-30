using CEC.Blazor.Data;
using System;

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
