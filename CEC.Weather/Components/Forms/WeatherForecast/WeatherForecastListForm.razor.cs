using Microsoft.AspNetCore.Components;
using CEC.Blazor.Components.BaseForms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using System.Threading.Tasks;
using CEC.Blazor.Components;
using CEC.Blazor.Components.UIControls;
using CEC.Blazor.Components.Modal;
using CEC.Weather.Views;

namespace CEC.Weather.Components
{
    public partial class WeatherForecastListForm : ListFormBase<DbWeatherForecast, WeatherForecastDbContext>
    {
        /// <summary>
        /// The Injected Controller service for this record
        /// </summary>
        [Inject]
        protected WeatherForecastControllerService ControllerService { get; set; }

        protected override Task OnRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Sets the specific service
                this.Service = this.ControllerService;
                // Sets the max column
                this.UIOptions.MaxColumn = 3;
            }
            return base.OnRenderAsync(firstRender);
        }

        /// <summary>
        /// Method called when the user clicks on a row in the viewer.
        /// </summary>
        /// <param name="id"></param>
        protected void OnView(int id)
        {
            if (this.UIOptions.UseModalViewer && this._BootstrapModal != null) this.OnModalAsync<WeatherForecastViewerForm>(id);
            else this.OnViewAsync<WeatherForecastViewerView>(id);
        }

        /// <summary>
        /// Method called when the user clicks on a row Edit button.
        /// </summary>
        /// <param name="id"></param>
        protected void OnEdit(int id)
        {
            if (this.UIOptions.UseModalViewer && this._BootstrapModal != null) this.OnModalAsync<WeatherForecastEditorForm>(id);
            else this.OnViewAsync<WeatherForecastEditorView>(id);
        }

}
}
