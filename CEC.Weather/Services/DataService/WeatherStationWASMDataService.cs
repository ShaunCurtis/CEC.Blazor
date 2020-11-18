using CEC.Weather.Data;
using CEC.Blazor.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using CEC.Blazor.Data;

namespace CEC.Weather.Services
{
    public class WeatherStationWASMDataService :
        BaseWASMDataService<DbWeatherStation, WeatherForecastDbContext>,
        IWeatherStationDataService
    {
        public WeatherStationWASMDataService(IConfiguration configuration, HttpClient httpClient) : base(configuration, httpClient)
        {
            this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherStation", RecordDescription = "Weather Station", RecordListName = "WeatherStation", RecordListDecription = "Weather Stations" };
        }
    }
}
