using CEC.Blazor.Data;
using CEC.Weather.Data;
using CEC.Blazor.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using CEC.Blazor.Components;

namespace CEC.Weather.Services
{
    public class WeatherStationServerDataService :
        BaseServerDataService<DbWeatherStation, WeatherForecastDbContext>,
        IWeatherStationDataService
    {
        public WeatherStationServerDataService(IConfiguration configuration, IDbContextFactory<WeatherForecastDbContext> dbcontext) : base(configuration, dbcontext)
        {
            this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherStation", RecordDescription = "Weather Station", RecordListName = "WeatherStation", RecordListDecription = "Weather Stations" };
        }

    }
}
