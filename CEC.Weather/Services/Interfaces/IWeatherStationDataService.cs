using CEC.Blazor.Services;
using CEC.Weather.Data;

namespace CEC.Weather.Services
{
    public interface IWeatherStationDataService : 
        IDataService<DbWeatherStation, WeatherForecastDbContext>
    {}
}
