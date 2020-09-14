using CEC.Blazor.Data;
using CEC.Weather.Data;
using CEC.Blazor.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;

namespace CEC.Weather.Services
{
    public class WeatherForecastWASMDataService : 
        BaseWASMDataService<DbWeatherForecast, WeatherForecastDbContext>,
        IWeatherForecastDataService

    {
         public override RecordConfigurationData RecordConfiguration => new RecordConfigurationData() { RecordName = "WeatherForecast", RecordDescription = "Weather Forecast", RecordListName = "WeatherForecasts", RecordListDecription = "Weather Forecasts" };

        public WeatherForecastWASMDataService(IConfiguration configuration, HttpClient httpClient) : base(configuration, httpClient) { }
         
    }
}
