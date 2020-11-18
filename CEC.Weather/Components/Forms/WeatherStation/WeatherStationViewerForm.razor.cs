using CEC.Blazor.Components.BaseForms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherStationViewerForm : RecordFormBase<DbWeatherStation, WeatherForecastDbContext>
    {
        [Inject]
        private WeatherStationControllerService ControllerService { get; set; }

        protected override Task OnRenderAsync(bool firstRender)
        {
            if (firstRender) this.Service = this.ControllerService;
            return base.OnRenderAsync(firstRender);
        }
    }
}
