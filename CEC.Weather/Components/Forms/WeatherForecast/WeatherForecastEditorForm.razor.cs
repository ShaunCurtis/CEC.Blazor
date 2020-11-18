using CEC.Blazor.Components.BaseForms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherForecastEditorForm : EditRecordFormBase<DbWeatherForecast, WeatherForecastDbContext>
    {
        [Inject]
        public WeatherForecastControllerService ControllerService { get; set; }

        protected override Task OnRenderAsync(bool firstRender)
        {
            // Assign the correct controller service
            if (firstRender) this.Service = this.ControllerService;
            return base.OnRenderAsync(firstRender);
        }
    }
}
