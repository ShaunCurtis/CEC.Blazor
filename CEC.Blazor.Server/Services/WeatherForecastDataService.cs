using CEC.Blazor.Data;
using CEC.Blazor.Server.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Services
{
    public class WeatherForecastDataService : BaseDataService<WeatherForecast>, IDbService<WeatherForecast>
    {

        private List<WeatherForecast> Records { get; set; }

        public WeatherForecastDataService(IConfiguration configuration) : base(configuration)
        {
            this.Configuration = new RecordConfigurationData() { RecordName = "WeatherForecast", RecordDescription = "Weather Forecast", RecordListName = "WeatherForecasts", RecordListDecription = "Weather Forecasts" };
            this.GetDummyRecords(100);
        }

        private void GetDummyRecords (int recordcount)
        {
            for(var i = 0; i > recordcount; i++ )
            {
                var rng = new Random();
                var temperatureC = rng.Next(-5, 35);
                var rec = new WeatherForecast()
                {
                    WeatherForecastID = i,
                    Date = DateTime.Now.AddDays(-(recordcount - i)),
                    TemperatureC = temperatureC,
                    Summary = (WeatherSummary)rng.Next(11),
                    Outlook = (WeatherOutlook)rng.Next(3),
                    Frost = temperatureC < 0,
                };
                Records.Add(rec);
            }
        }

        public Task<WeatherForecast> GetRecordAsync(int id) => Task.FromResult(this.Records.FirstOrDefault(item => item.WeatherForecastID == id));

        public Task<List<WeatherForecast>> GetRecordListAsync() => Task.FromResult(this.Records);

        public async Task<DbTaskResult> UpdateRecordAsync(WeatherForecast record)
        {
            var rec = await GetRecordAsync(record.WeatherForecastID);
            if (rec != null) rec = record;
            var result = new DbTaskResult() { IsOK = rec != null, NewID = 0 };
            result.Message = rec != null ? "Record Updated" : "Record could not be found";
            result.Type = rec != null ? MessageType.Success : MessageType.Error;
            return result;
        }

        public Task<DbTaskResult> AddRecordAsync(WeatherForecast record)
        {
            record.WeatherForecastID = this.Records.Max(item => item.WeatherForecastID) + 1;
            this.Records.Add(record);
            var result = new DbTaskResult() { IsOK = true, NewID = record.WeatherForecastID, Message = "Record Added", Type = MessageType.Success };
            return Task.FromResult(result);
        }

        public async Task<DbTaskResult> DeleteRecordAsync(int id)
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
