using CEC.Blazor.Components;
using CEC.Blazor.Server.Data;
using CEC.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Services
{
    public class WeatherForecastControllerService : BaseControlService<WeatherForecast>, IControlService<WeatherForecast>
    {

        /// <summary>
        /// Property exposing Error Trapped Record ID
        /// </summary>
        public override int RecordID => this.Record is null ? 0 : this.Record.WeatherForecastID;

        public WeatherForecastDataService WeatherForecastDataService { get; set; }

        public WeatherForecastControllerService(NavigationManager navmanager, IConfiguration appconfiguration, WeatherForecastDataService weatherForecastDataService) : base(appconfiguration, navmanager)
        {
            this.Service = weatherForecastDataService;
            this.WeatherForecastDataService = weatherForecastDataService;
            this.RecordConfiguration = weatherForecastDataService.Configuration;
        }

        /// <summary>
        /// Method to get a sorted Data Page for the List Page
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        public async override Task<List<WeatherForecast>> GetDataPageWithSortingAsync(PagingData<WeatherForecast> paging)
        {
            if (await this.GetFilteredListAsync()) paging.ResetRecordCount(this.Records.Count);
            if (paging.PageStartRecord > this.Records.Count) paging.CurrentPage = 1;
            if (string.IsNullOrEmpty(paging.SortColumn)) return this.Records.Skip(paging.PageStartRecord).Take(paging.PageSize).ToList();
            else
            {
                if (paging.SortingDirection == PagingData<WeatherForecast>.SortDirection.Ascending)
                {
                    return this.Records.OrderBy(x => x.GetType().GetProperty(paging.SortColumn).GetValue(x, null)).Skip(paging.PageStartRecord).Take(paging.PageSize).ToList();
                }
                else
                {
                    return this.Records.OrderByDescending(x => x.GetType().GetProperty(paging.SortColumn).GetValue(x, null)).Skip(paging.PageStartRecord).Take(paging.PageSize).ToList();
                }
            }
        }
    }
}
