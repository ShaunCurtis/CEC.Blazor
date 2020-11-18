using CEC.Blazor.Components;
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
    public class WeatherStationDummyDataService :
        BaseServerDataService<DbWeatherStation, WeatherForecastDbContext>,
        IDataService<DbWeatherStation, WeatherForecastDbContext>,
        IWeatherStationDataService
    {

        /// <summary>
        /// internal Property to hold the dummy records for CRUD operations
        /// </summary>
        private List<DbWeatherStation> Records { get; set; }

        public WeatherStationDummyDataService(IConfiguration configuration) : base(configuration, null)
        {
            this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherStation", RecordDescription = "Weather Station", RecordListName = "WeatherStation", RecordListDecription = "Weather Stations" };
            this.GetDummyRecords(100);
        }

        /// <summary>
        /// Method to get a set of 100 dummy records
        /// </summary>
        /// <param name="recordcount"></param>
        private void GetDummyRecords(int recordcount)
        {
            this.Records = new List<DbWeatherStation>();
            var rec = new DbWeatherStation()
            {
                ID = 1,
                Name = "Tiree",
                Longitude = -1.5m,
                Latitude = 54.2m,
                Elevation = 28

            };
            Records.Add(rec);
            rec = new DbWeatherStation()
            {
                ID = 2,
                Name = "Ross-on-Wye",
                Longitude = -1.2m,
                Latitude = 52.2m,
                Elevation = 120

            };
            Records.Add(rec);
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<List<DbWeatherStation>> GetRecordListAsync()
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
        public override Task<List<DbWeatherStation>> GetFilteredRecordListAsync(IFilterList filterList)
        {
            var firstrun = true;
            // Get a empty list
            var list = new List<DbWeatherStation>();
            list.AddRange(Records);
            // if we have a filter go through each filter
            if (filterList != null && filterList.Filters.Count > 0)
            {
                foreach (var filter in filterList.Filters)
                {
                    // Get the filter propertyinfo object
                    var x = typeof(DbWeatherStation).GetProperty(filter.Key);
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
        public override Task<DbWeatherStation> GetRecordAsync(int id) => Task.FromResult(this.Records.FirstOrDefault(item => item.ID == id));

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override async Task<DbTaskResult> UpdateRecordAsync(DbWeatherStation record)
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
        public override Task<DbTaskResult> CreateRecordAsync(DbWeatherStation record)
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
            //=> await this.DBContext.CreateDbContext().GetDistinctListAsync(req);
            return Task.FromResult(new List<string>());
        }

        /// <summary>
        /// Gets the Base Record list for the Datasat
        /// </summary>
        /// <returns></returns>
        public List<DbBaseRecord> GetBaseRecordList()
        {
            var list = new List<DbBaseRecord>();
            this.Records.ForEach(item => list.Add(new DbBaseRecord() { ID = item.ID, DisplayName = item.Name }));
            return list;
        }
        public override Task<List<DbBaseRecord>> GetBaseRecordListAsync<TLookup>()
        {
            var list = new List<DbBaseRecord>();
            if (typeof(TLookup).IsAssignableFrom(typeof(DbWeatherStation)))
            {
                return Task.FromResult(GetBaseRecordList());
            }
            return Task.FromResult(list);

        }


    }
}
