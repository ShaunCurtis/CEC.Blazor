using CEC.Blazor.Data;
using CEC.Blazor.Server.Data;
using CEC.Blazor.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Services
{
    public class WeatherForecastDataService : BaseDataService<DbWeatherForecast>, IDataService<DbWeatherForecast>
    {

        public WeatherForecastDataService(IConfiguration configuration) : base(configuration)
        {
            this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherForecast", RecordDescription = "Weather Forecast", RecordListName = "WeatherForecasts", RecordListDecription = "Weather Forecasts" };
        }

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

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbWeatherForecast> GetRecordAsync(int id) => await this.GetContext().WeatherForecasts.FirstOrDefaultAsync(item => item.WeatherForecastID == id);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<List<DbWeatherForecast>> GetRecordListAsync() => await this.GetContext().WeatherForecasts.ToListAsync();

        //public async Task<List<DbWeatherForecast>> GetRecordListAsync() {
        //    var recs =  await this.GetContext().WeatherForecasts.ToListAsync();
        //    if (recs.Count == 0) await this.GetDummyRecords(100);
        //    return recs;
        //}

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> UpdateRecordAsync(DbWeatherForecast record) => await this.GetContext().RunStoredProcedureAsync("sp_Update_WeatherForecast", this.GetSQLParameters(record, true), this.RecordConfiguration);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> AddRecordAsync(DbWeatherForecast record) => await this.GetContext().RunStoredProcedureAsync("sp_Add_WeatherForecast", this.GetSQLParameters(record, false), this.RecordConfiguration);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> DeleteRecordAsync(int id)
        {
            var parameters = new List<SqlParameter>() {
            new SqlParameter() {
                ParameterName =  "@WeatherForecastID",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Input,
                Value = id }
            };
            return await this.GetContext().RunStoredProcedureAsync("sp_Delete_WeatherForecast", parameters, this.RecordConfiguration);
        }

        /// <summary>
        /// Method that sets up the SQL Stored Procedure Parameters
        /// </summary>
        /// <param name="item"></param>
        /// <param name="withid"></param>
        /// <returns></returns>
        private List<SqlParameter> GetSQLParameters(DbWeatherForecast item, bool isinsert = false)
        {
            var parameters = new List<SqlParameter>() {
            new SqlParameter("@Date", SqlDbType.SmallDateTime) { Direction = ParameterDirection.Input, Value = item.Date.ToString("dd-MMM-yyyy") },
            new SqlParameter("@TemperatureC", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = item.TemperatureC },
            new SqlParameter("@Frost", SqlDbType.Bit) { Direction = ParameterDirection.Input, Value = item.Frost },
            new SqlParameter("@SummaryValue", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = item.SummaryValue },
            new SqlParameter("@OutlookValue", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = item.OutlookValue },
            new SqlParameter("@Description", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = item.Description },
            new SqlParameter("@PostCode", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = item.PostCode },
            new SqlParameter("@Detail", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = item.Detail },
            };
            if (isinsert) parameters.Insert(0, new SqlParameter("@WeatherForecastID", SqlDbType.BigInt) { Direction = ParameterDirection.Input, Value = item.ID });
            else parameters.Insert(0, new SqlParameter("@WeatherForecastID", SqlDbType.BigInt) { Direction = ParameterDirection.Output });
            return parameters;
        }

    }
}
