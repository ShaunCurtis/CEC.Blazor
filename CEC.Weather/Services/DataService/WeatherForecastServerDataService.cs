using CEC.Blazor.Data;
using CEC.Weather.Data;
using CEC.Blazor.Services;
using Microsoft.Extensions.Configuration;

namespace CEC.Weather.Services
{
    public class WeatherForecastServerDataService :
        BaseServerDataService<DbWeatherForecast, WeatherForecastDbContext>,
        IWeatherForecastDataService
    {
        public WeatherForecastServerDataService(IConfiguration configuration, IDbContextFactory<WeatherForecastDbContext> dbcontext) : base(configuration, dbcontext)
        {
            this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherForecast", RecordDescription = "Weather Forecast", RecordListName = "WeatherForecasts", RecordListDecription = "Weather Forecasts" };
        }
    }
}
