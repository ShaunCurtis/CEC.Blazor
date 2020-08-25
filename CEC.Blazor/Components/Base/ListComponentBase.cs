using CEC.Blazor.Data;
using CEC.Blazor.Services;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.Base
{
    public class ListComponentBase<T> : ControllerServiceComponentBase<T> where T : IDbRecord<T>, new()
    {

        /// <summary>
        /// Paging Interface giving access to the specific service that sorts the dataset into pages and tracks the page being displayed
        /// </summary>
        public IControllerPagingService<T> Paging => this.Service != null ? (IControllerPagingService<T>)this.Service : null;

        /// <summary>
        /// constructed Value for the List Title based on the RecordConfiguration
        /// </summary>
        public string ListTitle => this.IsService ? $"List of {this.Service.RecordConfiguration.RecordListDecription}" : "Record List"; 

        /// <summary>
        /// Should be overridden in inherited components
        /// and called after setting the Service
        /// </summary>
        protected async override Task OnInitializedAsync()
        {
            if (this.IsService)
            {
                await this.Service.Reset();
                await this.Service.LoadPagingAsync();
                this.Paging.PageHasChanged += UpdateUI;
                this.Service.ListHasChanged += this.OnRecordsUpdate;
            }
            await base.OnInitializedAsync();
            this.Loading = false;
        }

        /// <summary>
        /// Async Event Handler that resets the Paging record count and reloads the Pager
        /// Triggers a page reload when complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void OnRecordsUpdate(object sender, EventArgs e)
        {
            if (this.IsService)
            {
                this.Loading = false;
                await this.Paging.LoadAsync();
            }
            this.Loading = false;
            this.StateHasChanged();
        }

        /// <summary>
        /// Handler for a UpdatePage event - normally linked to the Pager PageHasChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recordno"></param>
        protected void UpdateUI(object sender, int recordno) => this.StateHasChanged();


        /// <summary>
        /// Event Handler for when the List is updated Externally i.e. not by the Pager 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual async void ListUpdated(object sender, EventArgs e)
        {
            await this.Paging.LoadAsync();
            this.StateHasChanged();
        }

        /// <summary>
        /// Event Handler normally linked to the Filter Control to force a reset of the Data Service Record List
        /// Which triggers a refresh of the pager
        /// </summary>
        /// <param name="filterlist"></param>
        protected virtual async void FilterUpdated(IFilterList filterlist) => await this.Service.ResetListAsync();

        public override void Dispose()
        {
            try
            {
                this.Paging.PageHasChanged -= UpdateUI;
            }
            catch { }
            base.Dispose();
        }
    }
}
