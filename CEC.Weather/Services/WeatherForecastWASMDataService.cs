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
using System.Net.Http;
using System.Net.Http.Json;

namespace CEC.Weather.Services
{
    public class WeatherForecastWASMDataService : BaseDataService<DbWeatherForecast>, IWeatherForecastDataService
    {
        protected HttpClient HttpClient { get; set; }

        public WeatherForecastWASMDataService(IConfiguration configuration, HttpClient httpClient) : base(configuration)
        {
            this.HttpClient = httpClient;
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
        public async Task<List<DbWeatherForecast>> GetRecordListAsync() => await this.HttpClient.GetFromJsonAsync<List<DbWeatherForecast>>("weatherforecast/list");

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetRecordListCountAsync() => await this.HttpClient.GetFromJsonAsync<int>("weatherforecast/count");

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> UpdateRecordAsync(DbWeatherForecast record)
        {
            var response = await this.HttpClient.PostAsJsonAsync<DbWeatherForecast>($"weatherforecast/update/", record);
            var result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            return result;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> AddRecordAsync(DbWeatherForecast record)
        {
            var response = await this.HttpClient.PostAsJsonAsync<DbWeatherForecast>($"weatherforecast/update/", record);
            var result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            return result;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> DeleteRecordAsync(int id)
        {
            var response = await this.HttpClient.PostAsJsonAsync<int>($"weatherforecast/update/", id);
            var result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            return result;
        }

    }
}
