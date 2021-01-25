using Microsoft.AspNetCore.Components;
using CEC.Blazor.SPA.Components.Forms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using CEC.Weather.Components.Views;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherReportListForm : ListFormBase<DbWeatherReport, WeatherForecastDbContext>
    {
        /// <summary>
        /// The Injected Controller service for this record
        /// </summary>
        [Inject]
        protected WeatherReportControllerService ControllerService { get; set; }

        [Parameter]
        public int WeatherStationID { get; set; }

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
            if (this.UIOptions.UseModalViewer && this.ViewManager.ModalDialog != null) this.OnModalAsync<WeatherForecastViewerForm>(id);
            else this.OnViewAsync<WeatherReportViewerView>(id);
        }

        /// <summary>
        /// Method called when the user clicks on a row Edit button.
        /// </summary>
        /// <param name="id"></param>
        protected void OnEdit(int id)
        {
            if (this.UIOptions.UseModalViewer && this.ViewManager.ModalDialog != null) this.OnModalAsync<WeatherForecastEditorForm>(id);
            else this.OnViewAsync<WeatherReportEditorView>(id);
        }

        /// <summary>
        /// inherited - loads the filter
        /// </summary>
        protected override void LoadFilter()
        {
            ((CEC.Blazor.Services.IControllerPagingService<DbWeatherReport>)this.Service).DefaultSortColumn = "Date";
            // Before the call to base so the filter is set before the get the list
            if (this.IsService &&  this.WeatherStationID > 0)
            {
                this.Service.FilterList.Filters.Clear();
                this.Service.FilterList.Filters.Add("WeatherStationID", this.WeatherStationID);
            }
            base.LoadFilter();
        }
    }
}
