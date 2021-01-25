using CEC.Blazor.SPA.Components;
using CEC.Blazor.Data;
using CEC.Weather.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Weather.Services
{
    public class WeatherForecastDummyDataService :
        BaseServerDataService<DbWeatherForecast, WeatherForecastDbContext>,
        IWeatherForecastDataService
    {

        /// <summary>
        /// internal Property to hold the dummy records for CRUD operations
        /// </summary>
        private List<DbWeatherForecast> Records { get; set; }

        public WeatherForecastDummyDataService(IConfiguration configuration) : base(configuration, null)
        {
            this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherForecast", RecordDescription = "Weather Forecast", RecordListName = "WeatherForecasts", RecordListDecription = "Weather Forecasts" };
            this.GetDummyRecords(100);
        }

        /// <summary>
        /// Method to get a set of 100 dummy records
        /// </summary>
        /// <param name="recordcount"></param>
        private void GetDummyRecords(int recordcount)
        {
            this.Records = new List<DbWeatherForecast>();
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
                Records.Add(rec);
            }
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Task<DbWeatherForecast> GetRecordAsync(int id)
        {
            return Task.FromResult(this.Records.FirstOrDefault(item => item.ID == id));
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public override async Task<List<DbWeatherForecast>> GetRecordListAsync()
        {
            // Delay to demonstrate Async Programming
            await Task.Delay(2000);
            return this.Records;
        }

        public override async Task<List<DbWeatherForecast>> GetFilteredRecordListAsync(IFilterList filterList) => await GetRecordListAsync();

        public override Task<int> GetRecordListCountAsync() => Task.FromResult(Records.Count());

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override async Task<DbTaskResult> UpdateRecordAsync(DbWeatherForecast record)
        {
            var rec = await GetRecordAsync(record.ID);
            if (rec != null) rec = record;
            var result = new DbTaskResult() { IsOK = rec != null, NewID = 0 };
            result.Message = rec != null ? "Record Updated" : "Record could not be found";
            result.Type = rec != null ? MessageType.Success : MessageType.Error;
            return result;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override Task<DbTaskResult> CreateRecordAsync(DbWeatherForecast record)
        {
            record.ID = this.Records.Max(item => item.ID) + 1;
            this.Records.Add(record);
            var result = new DbTaskResult() { IsOK = true, NewID = record.ID, Message = "Record Added", Type = MessageType.Success };
            return Task.FromResult(result);
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<DbTaskResult> DeleteRecordAsync(int id)
        {
            var rec = await GetRecordAsync(id);
            var isrecord = rec != null;
            if (isrecord) this.Records.Remove(rec);
            var result = new DbTaskResult() { IsOK = isrecord, NewID = 0 };
            result.Message = isrecord ? "Record Deleted" : "Record could not be found";
            result.Type = isrecord ? MessageType.Success : MessageType.Error;
            return result;
        }
    }
}
