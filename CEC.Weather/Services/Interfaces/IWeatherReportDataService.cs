using CEC.Blazor.Services;
using CEC.Weather.Data;

namespace CEC.Weather.Services
{
    public interface IWeatherReportDataService : 
        IDataService<DbWeatherReport, WeatherForecastDbContext>
    {
    }
}
