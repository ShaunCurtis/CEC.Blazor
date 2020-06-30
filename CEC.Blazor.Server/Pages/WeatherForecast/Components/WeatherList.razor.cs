using Microsoft.AspNetCore.Components;
using CEC.Blazor.Server.Data;
using CEC.Blazor.Components.Base;
using CEC.Blazor.Server.Services;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Pages.Components
{
    public partial class WeatherList : ListComponentBase<WeatherForecast>
    {
        [Inject]
        protected WeatherForecastControllerService ControllerService { get; set; }

        protected override void OnInitialized()
        {
            this.Service = this.ControllerService;
            base.OnInitialized();
            this.ControllerService.ListHasChanged += this.OnRecordsUpdate;
        }
    }
}
