using CEC.Blazor.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVC = Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using CEC.Weather.Data;
using Microsoft.IdentityModel.Tokens;
using CEC.Blazor.Data;

namespace CEC.Blazor.WASM.Server.Controllers
{
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {

        protected IWeatherForecastDataService DataService { get; set; }

        private readonly ILogger<WeatherForecastController> logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastDataService weatherForecastDataService)
        {
            this.DataService = weatherForecastDataService;
            this.logger = logger;
        }

        [MVC.Route("weatherforecast/list")]
        [HttpGet]
        public async Task<List<DbWeatherForecast>> GetList()
        {
            return await DataService.GetRecordListAsync();
        }

        [MVC.Route("weatherforecast/count")]
        [HttpGet]
        public async Task<int> Count()
        {
            return await DataService.GetRecordListCountAsync();
        }

        [MVC.Route("weatherforecast/update")]
        [HttpPost]
        public async Task<DbTaskResult> Update(DbWeatherForecast record)
        {
            return await DataService.UpdateRecordAsync(record);
        }

        [MVC.Route("weatherforecast/create")]
        [HttpPost]
        public async Task<DbTaskResult> Create(DbWeatherForecast record)
        {
            return await DataService.AddRecordAsync(record);
        }

        [MVC.Route("weatherforecast/delete")]
        [HttpPost]
        public async Task<DbTaskResult> Delete(int id)
        {
            return await DataService.DeleteRecordAsync(id);
        }
    }
}
