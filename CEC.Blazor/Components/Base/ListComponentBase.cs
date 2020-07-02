using CEC.Blazor.Data;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.Base
{
    public class ListComponentBase<T> : ControllerServiceComponentBase<T> where T : IDbRecord<T>, new()
    {

        /// <summary>
        /// Paging object that sorts the dataset into pages and tracks the page being displayed
        /// </summary>
        public PagingData<T> Paging { get; set; }

        /// <summary>
        /// Used by Button Controls to checking if to display control or not if bottons are disabled
        /// </summary>
        public bool IsPagination => this.Paging != null && this.Paging.IsPagination;

        /// <summary>
        /// constructed Value for the List Title based on the RecordConfiguration
        /// </summary>
        public string ListTitle => this.IsService ? $"List of {this.Service.RecordConfiguration.RecordListDecription}" : "Record List"; 

        /// <summary>
        /// Used to determine if the page can display data
        /// </summary>
        public bool IsError => !(this.Paging != null && this.Service != null && this.Paging.Records != null);

        /// <summary>
        /// Should be overridden in inherited components
        /// and called after setting the Service
        /// </summary>
        protected async override Task OnInitializedAsync()
        {
            this.Paging = null;
            if (this.IsService)
            {
                this.Service.Reset();
                await this.LoadPagingAsync();
            }
            this.Service.ListHasChanged += this.OnRecordsUpdate;
            await base.OnInitializedAsync();
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
                this.Paging.ResetRecordCount(Service.RecordCount);
                await this.Paging.LoadAsync();
            }
            this.StateHasChanged();
        }

        /// <summary>
        /// Method to load up the Pager.  Gets a new PaginationData object, 
        /// loads the delegate with the default service GetDataPage method and loads the first page
        /// Can be overrideden for more complex situations
        /// </summary>
        protected async virtual Task LoadPagingAsync(bool withDelegate = true)
        {
            if (this.IsService)
            {
                // set the record to null to force a reload of the records
                Service.Records = null;
                // Cresates a new paging data object
                this.Paging = new PagingData<T>(Service.DefaultPageSize, Service.RecordCount);
                // if requested adds a default service function to the delegate
                if (withDelegate)
                {
                    this.Paging.PageLoaderAsync = new PagingData<T>.PageLoaderDelegateAsync(Service.GetDataPageAsync);
                    // loads the paging object
                    await this.Paging.LoadAsync();
                    // forces a UI update
                    this.UpdateState();
                }
                // links the page p=changed event to update the UI
                this.Paging.PageHasChanged += UpdateUI;
            }
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
            this.Paging.PageHasChanged -= UpdateUI;
            base.Dispose();
        }

    }
}
