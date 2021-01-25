using CEC.Blazor.SPA.Components.Forms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherReportViewerForm : RecordFormBase<DbWeatherReport, WeatherForecastDbContext>
    {
        [Inject]
        private WeatherReportControllerService ControllerService { get; set; }

        protected override Task OnRenderAsync(bool firstRender)
        {
            if (firstRender) this.Service = this.ControllerService;
            return base.OnRenderAsync(firstRender);
        }
    }
}
