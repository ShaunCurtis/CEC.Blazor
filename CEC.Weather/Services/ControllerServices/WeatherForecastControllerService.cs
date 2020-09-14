using CEC.Blazor.Components;
using CEC.Weather.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CEC.Weather.Services
{
    public class WeatherForecastControllerService : BaseControllerService<DbWeatherForecast, WeatherForecastDbContext>, IControllerService<DbWeatherForecast, WeatherForecastDbContext>
    {

        /// <summary>
        /// List of Outlooks for Select Controls
        /// </summary>
        public SortedDictionary<int, string> OutlookOptionList => Utils.GetEnumList<WeatherOutlook>();

        public WeatherForecastControllerService(NavigationManager navmanager, IConfiguration appconfiguration, IWeatherForecastDataService weatherForecastDataService) : base(appconfiguration, navmanager)
        {
            this.Service = weatherForecastDataService;
            this.DefaultSortColumn = "WeatherForecastID";
        }
    }
}
