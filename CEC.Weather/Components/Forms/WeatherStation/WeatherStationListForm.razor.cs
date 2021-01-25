using Microsoft.AspNetCore.Components;
using CEC.Blazor.SPA.Components.Forms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using CEC.Weather.Components.Views;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherStationListForm : ListFormBase<DbWeatherStation, WeatherForecastDbContext>
    {
        /// <summary>
        /// The Injected Controller service for this record
        /// </summary>
        [Inject]
        protected WeatherStationControllerService ControllerService { get; set; }

        protected override Task OnRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Sets the specific service
                this.Service = this.ControllerService;
                // Sets the max column
                this.UIOptions.MaxColumn = 2;
            }
            return base.OnRenderAsync(firstRender);
        }

        /// <summary>
        /// Method called when the user clicks on a row in the viewer.
        /// </summary>
        /// <param name="id"></param>
        protected void OnView(int id)
        {
            if (this.UIOptions.UseModalViewer && this.ViewManager.ModalDialog != null) this.OnModalAsync<WeatherStationViewerForm>(id);
            else this.OnViewAsync<WeatherStationViewerView>(id);
        }

        /// <summary>
        /// Method called when the user clicks on a row Edit button.
        /// </summary>
        /// <param name="id"></param>
        protected void OnEdit(int id)
        {
            if (this.UIOptions.UseModalViewer && this.ViewManager.ModalDialog != null) this.OnModalAsync<WeatherStationEditorForm>(id);
            else this.OnViewAsync<WeatherStationEditorView>(id);
        }

    }
}
