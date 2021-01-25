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
    public class WeatherReportDummyDataService :
        BaseServerDataService<DbWeatherReport, WeatherForecastDbContext>,
        IDataService<DbWeatherReport, WeatherForecastDbContext>,
        IWeatherReportDataService
    {

        /// <summary>
        /// internal Property to hold the dummy records for CRUD operations
        /// </summary>
        private List<DbWeatherReport> Records { get; set; }

        private IWeatherStationDataService stationService { get; set; }

        public WeatherReportDummyDataService(IConfiguration configuration, IWeatherStationDataService weatherStationDataService) : base(configuration, null)
        {
            this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherStation", RecordDescription = "Weather Station", RecordListName = "WeatherStation", RecordListDecription = "Weather Stations" };
            this.GetDummyRecords();
            this.stationService = weatherStationDataService;
        }

        private void GetDummyRecords()
        {
            this.Records = new List<DbWeatherReport>();
            var i = 1;
            for (var id = 1; id <= 2; id++)
            {
                var stationname = "Tiree";
                var date = new DateTime(1970, 1, 1);
                if (id == 2) stationname = "Ross-on-Wye";
                while (date < DateTime.Now)
                {
                    var rng = new Random();
                    var tempmin = rng.Next(-5, 18);
                    var tempmax = tempmin + rng.Next(0, 18);
                    var rec = new DbWeatherReport()
                    {
                        ID = i++,
                        WeatherStationID = id,
                        WeatherStationName = stationname,
                        Date = date,
                        TempMin = tempmin,
                        TempMax = tempmax,
                        FrostDays = rng.Next(0, 20),
                        Rainfall = rng.Next(0, 200),
                        SunHours = rng.Next(0, 200),
                        Month = date.Month,
                        Year = date.Year,
                        DisplayName = $"Record for {date.Month}-{date.Year}"
                    };
                    Records.Add(rec);
                    date = date.AddMonths(1);
                }
            }
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public override async Task<List<DbWeatherReport>> GetRecordListAsync()
        {
            // Delay to demonstrate Async Programming
            await Task.Delay(200);
            return this.Records;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public override Task<int> GetRecordListCountAsync() => Task.FromResult(this.Records.Count);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public override Task<List<DbWeatherReport>> GetFilteredRecordListAsync(IFilterList filterList)
        {
            var firstrun = true;
            // Get a empty list
            var list = new List<DbWeatherReport>();
            list.AddRange(Records);
            // if we have a filter go through each filter
            if (filterList != null && filterList.Filters.Count > 0)
            {
                foreach (var filter in filterList.Filters)
                {
                    // Get the filter propertyinfo object
                    var x = typeof(DbWeatherReport).GetProperty(filter.Key);
                    // if we have a list already apply the filter to the list
                    if (list.Count > 0) list = list.Where(item => x.GetValue(item).Equals(filter.Value)).ToList();
                    // If this is the first run we query the database directly
                    else if (firstrun) list = this.Records.Where(item => x.GetValue(item).Equals(filter.Value)).ToList();
                    firstrun = false;
                }
            }
            return Task.FromResult(list);
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Task<DbWeatherReport> GetRecordAsync(int id) => Task.FromResult(this.Records.FirstOrDefault(item => item.ID == id));

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override async Task<DbTaskResult> UpdateRecordAsync(DbWeatherReport record)
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
        public override Task<DbTaskResult> CreateRecordAsync(DbWeatherReport record)
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


        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public override Task<List<string>> GetDistinctListAsync(DbDistinctRequest req)
        {
            var x = typeof(DbWeatherStation).GetProperty(req.FieldName);
            var list = this.Records.Select(item => item.Year.ToString()).Distinct().ToList();
            //=> await this.DBContext.CreateDbContext().GetDistinctListAsync(req);
            return Task.FromResult(list);
        }

        public override async Task<List<DbBaseRecord>> GetBaseRecordListAsync<TLookup>()
        {
            var list = new List<DbBaseRecord>();
            if (typeof(TLookup).IsAssignableFrom(typeof(DbWeatherStation)))
            {
                list = await stationService.GetBaseRecordListAsync<DbWeatherStation>();
            }
            return list;
        }


    }
}
