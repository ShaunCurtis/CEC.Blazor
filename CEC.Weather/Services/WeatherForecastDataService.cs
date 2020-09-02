using CEC.Blazor.Data;
using CEC.Weather.Data;
using CEC.Blazor.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CEC.Weather.Services
{
    public class WeatherForecastDataService : BaseDataService<DbWeatherForecast>, IWeatherForecastDataService
    {

        public WeatherForecastDataService(IConfiguration configuration) : base(configuration) { }

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

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetRecordListCountAsync() => await this.GetContext().WeatherForecasts.CountAsync();

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
