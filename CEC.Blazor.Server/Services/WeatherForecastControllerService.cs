using CEC.Blazor.Components;
using CEC.Blazor.Server.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CEC.Blazor.Server.Services
{
    public class WeatherForecastControllerService : BaseControllerService<DbWeatherForecast>, IControllerService<DbWeatherForecast>
    {

        /// <summary>
        /// List of Outlooks for Select Controls
        /// </summary>
        public SortedDictionary<int, string> OutlookOptionList => Utils.GetEnumList<WeatherOutlook>();

        public WeatherForecastControllerService(NavigationManager navmanager, IConfiguration appconfiguration, WeatherForecastDataService weatherForecastDataService) : base(appconfiguration, navmanager)
        {
            this.Service = weatherForecastDataService;
            this.DefaultSortColumn = "WeatherForecastID";
        }
    }
}
