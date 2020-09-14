using CEC.Blazor.Data;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace CEC.Blazor.Services
{
    public class BaseWASMDataService<TRecord, TContext> : 
        BaseDataService<TRecord, TContext>, 
        IDataService<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext
    {
 
        public override RecordConfigurationData RecordConfiguration { get; set; } = new RecordConfigurationData();

        public BaseWASMDataService(IConfiguration configuration, HttpClient httpClient): base(configuration) => this.HttpClient = httpClient;

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TRecord> GetRecordAsync(int id)
        {
            //return await this.HttpClient.GetFromJsonAsync<DbWeatherForecast>($"weatherforecast/getrec?id={id}");
            var response = await this.HttpClient.PostAsJsonAsync($"{RecordConfiguration.RecordName}/read", id);
            var result = await response.Content.ReadFromJsonAsync<TRecord>();
            return result;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<List<TRecord>> GetRecordListAsync() => await this.HttpClient.GetFromJsonAsync<List<TRecord>>($"{RecordConfiguration.RecordName}/list");

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetRecordListCountAsync() => await this.HttpClient.GetFromJsonAsync<int>($"{RecordConfiguration.RecordName}/count");

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> UpdateRecordAsync(TRecord record)
        {
            var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{RecordConfiguration.RecordName}/update", record);
            var result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            return result;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> CreateRecordAsync(TRecord record)
        {
            var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{RecordConfiguration.RecordName}/create", record);
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
            var response = await this.HttpClient.PostAsJsonAsync<int>($"{RecordConfiguration.RecordName}/update", id);
            var result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            return result;
        }
    }
}
