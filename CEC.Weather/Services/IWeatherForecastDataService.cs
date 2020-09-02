using CEC.Blazor.Services;
using CEC.Weather.Data;
using System.Collections.Generic;
using CEC.Blazor.Utilities;
using System.Threading.Tasks;
using CEC.Blazor.Data;
using System;

namespace CEC.Weather.Services
{
    public interface IWeatherForecastDataService : IDataService<DbWeatherForecast>
    {
        public new RecordConfigurationData RecordConfiguration => new RecordConfigurationData() { RecordName = "WeatherForecast", RecordDescription = "Weather Forecast", RecordListName = "WeatherForecasts", RecordListDecription = "Weather Forecasts" };

        /// <summary>
        /// Method to get a set of 100 dummy records
        /// </summary>
        /// <param name="recordcount"></param>
        private async Task<List<DbWeatherForecast>> GetDummyRecords(int recordcount)
        {
            var recs = new List<DbWeatherForecast>();
            for (var i = 1; i <= recordcount; i++)
            {
                var rng = new Random();
                var temperatureC = rng.Next(-5, 35);
                var rec = new DbWeatherForecast()
                {
                    WeatherForecastID = i,
                    Date = DateTime.Now.AddDays(-(recordcount - i)),
                    TemperatureC = temperatureC,
                    Summary = (WeatherSummary)rng.Next(11),
                    Outlook = (WeatherOutlook)rng.Next(3),
                    Frost = temperatureC < 0,
                    PostCode = "GL2 5TP"
                };
                rec.Description = $"The Weather forecast for {rec.Date.DayOfWeek} {rec.Date.ToLongDateString()} is mostly {rec.Outlook} and {rec.Summary}";
                await this.AddRecordAsync(rec);
                recs.Add(rec);
            }
            return recs;
        }


    }
}
