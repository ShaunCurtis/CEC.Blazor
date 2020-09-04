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
    public class WeatherForecastServerDataService : BaseServerDataService<DbWeatherForecast>, IWeatherForecastDataService
    {

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

        public WeatherForecastServerDataService(IConfiguration configuration) : base(configuration) { }

    }
}
