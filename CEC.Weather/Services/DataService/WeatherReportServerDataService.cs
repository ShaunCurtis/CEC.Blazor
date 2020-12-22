using CEC.Blazor.Data;
using CEC.Weather.Data;
using CEC.Blazor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CEC.Weather.Services
{
    public class WeatherReportServerDataService :
        BaseServerDataService<DbWeatherReport, WeatherForecastDbContext>,
        IWeatherReportDataService
    {
        public WeatherReportServerDataService(IConfiguration configuration, IDbContextFactory<WeatherForecastDbContext> dbcontext) : base(configuration, dbcontext)
        {
            this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherReport", RecordDescription = "Weather Report", RecordListName = "WeatherReport", RecordListDecription = "Weather Reports" };
        }
    }
}
