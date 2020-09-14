using CEC.Blazor.Services;
using CEC.Weather.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using CEC.Blazor.Data;

namespace CEC.Weather.Services
{
    public interface IWeatherForecastDataService : 
        IDataService<DbWeatherForecast, WeatherForecastDbContext>
    {
        /// <summary>
        /// Method to get a set of 100 dummy records
        /// </summary>
        /// <param name="recordcount"></param>
        public async Task<List<DbWeatherForecast>> GetDummyRecords(int recordcount)
        {
            var recs = new List<DbWeatherForecast>();
            for (var i = 1; i <= recordcount; i++)
            {
                var rng = new Random();
                var temperatureC = rng.Next(-5, 35);
                var rec = new DbWeatherForecast()
                {
                    ID = i,
                    Date = DateTime.Now.AddDays(-(recordcount - i)),
                    TemperatureC = temperatureC,
                    Summary = (WeatherSummary)rng.Next(11),
                    Outlook = (WeatherOutlook)rng.Next(3),
                    Frost = temperatureC < 0,
                    PostCode = "GL2 5TP"
                };
                rec.Description = $"The Weather forecast for {rec.Date.DayOfWeek} {rec.Date.ToLongDateString()} is mostly {rec.Outlook} and {rec.Summary}";
                await this.CreateRecordAsync(rec);
                recs.Add(rec);
            }
            return recs;
        }
    }
}
