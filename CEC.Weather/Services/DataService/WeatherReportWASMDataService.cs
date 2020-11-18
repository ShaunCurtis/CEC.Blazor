using CEC.Weather.Data;
using CEC.Blazor.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using CEC.Blazor.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace CEC.Weather.Services
{
    public class WeatherReportWASMDataService :
        BaseWASMDataService<DbWeatherReport, WeatherForecastDbContext>,
        IWeatherReportDataService
    {
        public WeatherReportWASMDataService(IConfiguration configuration, HttpClient httpClient) : base(configuration, httpClient)
        {
            this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherReport", RecordDescription = "Weather Report", RecordListName = "WeatherReport", RecordListDecription = "Weather Reports" };
        }

        //public override async Task<List<DbBaseRecord>> GetBaseRecordListAsync<TLookup>()
        //{
        //    var recordname = typeof(TLookup).Name.Replace("Db", "", System.StringComparison.CurrentCultureIgnoreCase);
        //    return await this.HttpClient.GetFromJsonAsync<List<DbBaseRecord>>($"{recordname}/base");
        //}

    }
}
