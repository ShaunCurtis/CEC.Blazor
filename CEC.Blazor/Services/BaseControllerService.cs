using CEC.Blazor.Components;
using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Blazor.Services
{
    public abstract class BaseControllerService<T> : IDisposable, IControllerPagingService<T>, IControllerService<T> where T : IDbRecord<T>, new()
    {
        #region Properties

        /// <summary>
        /// Unique ID for the Service
        /// Helps in debugging
        /// </summary>
        public Guid ID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Corresponding Data Service for Type T
        /// </summary>
        public IDataService<T> Service { get; set; }

        /// <summary>
        /// Access to the Application Configuration data
        /// </summary>
        public IConfiguration AppConfiguration { get; set; }

        /// <summary>
        /// Access to the Navigation Manager
        /// </summary>
        public NavigationManager NavManager { get; set; }

        /// <summary>
        /// public Property of DbTaskResult set when a CRUD operation is called
        /// The UI can build an alert/confirmation method from the information provided
        /// </summary>
        public DbTaskResult TaskResult { get; protected set; } = new DbTaskResult();

        /// <summary>
        /// Property that gets the page size to use in Paging operations
        /// Reads it from the application configuration file
        /// </summary>
        public int DefaultPageSize => int.TryParse(this.AppConfiguration["Paging:PageSize"], out int value) ? value : 20;

        /// <summary>
        /// The record Configuration - read from the data service
        /// </summary>
        public RecordConfigurationData RecordConfiguration => this.Service?.RecordConfiguration ?? new RecordConfigurationData();

        /// <summary>
        /// The Current Record
        /// Logic ensures a record always exists and the Shadow copy is up to date
        /// </summary>
        public T Record
        {
            get
            {
                if (this._Record is null) this._Record = new T();
                return this._Record;
            }

            set => this._Record = value;
        }

        /// <summary>
        /// The actual Current Record
        /// </summary>
        protected T _Record { get; private set; }

        /// <summary>
        /// Shadow Copy of the Current Record
        /// Should always be an unaltered copy of what's in the database
        /// Only used by Editors
        /// </summary>
        public T ShadowRecord { set; get; } = new T();

        /// <summary>
        /// Public Current List of Records used for Listing Operations
        /// </summary>
        public List<T> Records
        {
            get => this._Records;
            set => this._Records = value;
        }

        /// <summary>
        /// The Actual Current List of Records
        /// </summary>
        public List<T> _Records { get; private set; }

        /// <summary>
        /// Used by the list methods to filter the list contents.
        /// </summary>
        public virtual FilterList FilterList { get; set; }

        /// <summary>
        /// Abstract Property to expose the Record ID
        /// Implemented in inherited classes with error checking for Record Exists
        /// </summary>
        public abstract int RecordID { get; }

        /// <summary>
        /// Boolean Property to check if a real record exists 
        /// </summary>
        public virtual bool IsRecord => this.Record != null;

        /// <summary>
        /// Boolean Property to check if an Edit record exists 
        /// </summary>
        public virtual bool IsEditRecord => this.IsRecord && this.RecordID > -1;

        /// <summary>
        /// Boolean Property to check if a New record exists 
        /// </summary>
        public virtual bool IsNewRecord => this.IsRecord && this.RecordID == 0;

        /// <summary>
        /// Property exposing the number of records in the current list
        /// </summary>
        public int RecordCount => this.Records?.Count ?? 0;

        /// <summary>
        /// Boolean Property used to check if the record list exists
        /// </summary>
        public bool IsRecords => this.RecordCount > 0;

        /// <summary>
        /// Boolean Property used to check if the Data Service is set
        /// </summary>
        public bool IsService => this.Service != null;

        /// <summary>
        /// Property exposing the current save state of the record 
        /// </summary>
        public bool IsClean { get; protected set; } = true;

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the record is edited and not saved
        /// </summary>
        public event EventHandler OnDirty;

        /// <summary>
        /// Event triggered when the record is saved
        /// </summary>
        public event EventHandler OnClean;

        /// <summary>
        /// Event triggered when the record has changed
        /// </summary>
        public event EventHandler RecordHasChanged;

        /// <summary>
        /// Event triggered whwen the list has changed
        /// </summary>
        public event EventHandler ListHasChanged;

        #endregion

        public BaseControllerService(IConfiguration configuration, NavigationManager navigationManager)
        {
            this.NavManager = navigationManager;
            this.AppConfiguration = configuration;
        }

        /// <summary>
        /// Method to reset the Service to New
        /// May need to overridden in more complex services
        /// Called in edit mode when user navigates to New
        /// </summary>
        public virtual void Reset()
        {
            this.FilterList = new FilterList();
            this.Record = new T();
            this.ShadowRecord = new T();
            this.Records = new List<T>();
            this.SetClean();
        }

        /// <summary>
        /// Method to trigger a Record Changed Event
        /// </summary>
        public virtual void TriggerRecordChangedEvent(object sender) => this.RecordHasChanged?.Invoke(sender, EventArgs.Empty);

        /// <summary>
        /// Method to trigger a List Changed Event
        /// </summary>
        public virtual void TriggerListChangedEvent(object sender) => this.ListHasChanged?.Invoke(sender, EventArgs.Empty);

        /// <summary>
        /// Reset the list to an empty List
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ResetList(object sender, EventArgs e)
        {
            Records = new List<T>();
            this.TriggerListChangedEvent(sender);
        }

        /// <summary>
        /// Reset the List to null  We set to null because:
        /// A null list means the list needs reloading.  It's how the Paging system knows to reload the list
        /// An empty list may be a no results returned list
        /// </summary>
        public virtual void ResetList()
        {
            this.Records = null;
            this.TriggerListChangedEvent(this);
        }

        /// <summary>
        /// Async Reset the List to null  We set to null because:
        /// A null list means the list needs reloading.  It's how the Paging system knows to reload the list
        /// An empty list may be a no results returned list
        /// </summary>
        public virtual Task ResetListAsync()
        {
            this.Records = null;
            this.TriggerListChangedEvent(this);
            return Task.CompletedTask;
        }

        public virtual Task<bool> GetNewRecordAsync()
        {
            this.Record = new T();
            this.ShadowRecord = new T();
            return Task.FromResult(true);
        }

        /// <summary>
        /// Method to set the state to clean
        /// </summary>
        public void SetClean(bool isClean = true)
        {
            this.IsClean = isClean;
            this.OnClean?.Invoke(this, EventArgs.Empty);

        }

        /// <summary>
        /// Method to set the state to dirty
        /// </summary>
        public void SetDirty()
        {
            this.IsClean = false;
            this.OnDirty?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Method to get a data page for a list - used in Pagination
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        public async virtual Task<List<T>> GetDataPageAsync()
        {
            await this.GetFilteredListAsync();
            if (this.PageStartRecord < this.Records.Count) return this.Records.GetRange(this.PageStartRecord, this.PageGetSize);
            return new List<T>();
        }

        /// <summary>
        /// Base Implementation of the sorted version of paging.  Should be onveridden in descendants
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        public async virtual Task<List<T>> GetDataPageWithSortingAsync()
        {
            await this.GetFilteredListAsync();
            if (this.PageStartRecord < this.Records.Count) return this.Records.GetRange(this.PageStartRecord, this.PageGetSize);
            return new List<T>();
        }

        /// <summary>
        ///Base implementation that gets the full list.  Override for specific filtering
        /// </summary>
        protected async virtual Task<bool> GetFilteredListAsync()
        {
            if (!this.IsRecords)
            {
                //this.TriggerListChangedEvent(this);
                this.Records = await this.Service.GetRecordListAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Async Gets a record from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task<bool> GetRecordAsync(int? id)
        {
            if (this.IsService)
            {
                this.Record = await this.Service.GetRecordAsync(id ?? 0);
                this.Record ??= new T();
                this.ShadowRecord = this.Record.ShadowCopy();
                this.SetClean();
                this.TriggerRecordChangedEvent(this);
            }
            return this.IsRecord;
        }

        /// <summary>
        /// Saves a record - adds if new, otherwise updates the existing record
        /// </summary>
        /// <returns></returns>
        public async virtual Task<bool> SaveRecordAsync()
        {
            if (this.Record.ID > 0) return await UpdateRecordAsync();
            else return await AddRecordAsync();
        }

        /// <summary>
        /// Updates a record in the database
        /// </summary>
        /// <returns></returns>
        public async virtual Task<bool> UpdateRecordAsync()
        {
            this.TaskResult = await this.Service.UpdateRecordAsync(this.Record);
            if (this.TaskResult.IsOK)
            {
                this.ShadowRecord = this.Record.ShadowCopy();
                this.SetClean();
                this.TriggerRecordChangedEvent(this);
                this.TriggerListChangedEvent(this);
            }
            return TaskResult.IsOK;
        }

        /// <summary>
        /// Adds a record to the Database
        /// </summary>
        /// <returns></returns>
        public async virtual Task<bool> AddRecordAsync()
        {
            this.TaskResult = await this.Service.AddRecordAsync(this.Record);
            if (this.TaskResult.IsOK)
            {
                this.Record = await this.Service.GetRecordAsync(this.TaskResult.NewID);
                this.ShadowRecord = this.Record.ShadowCopy();
                this.SetClean();
                this.TriggerRecordChangedEvent(this);
                this.TriggerListChangedEvent(this);
            }
            return TaskResult.IsOK && this.IsRecord;
        }

        /// <summary>
        /// Method to load up the Paged Data to display
        /// loads the delegate with the default service GetDataPage method and loads the first page
        /// Can be overridden for more complex situations
        /// </summary>
        public async virtual Task LoadPagingAsync(bool withDelegate = true)
        {
            // set the record to null to force a reload of the records
            this.Records = null;
            // if requested adds a default service function to the delegate
            if (withDelegate)
            {
                this.PageLoaderAsync = new IControllerPagingService<T>.PageLoaderDelegateAsync(this.GetDataPageAsync);
                // loads the paging object
                await this.LoadAsync();
                this.TriggerListChangedEvent(this);
            }
        }

        /// <summary>
        /// Pseudo Dispose Event - not currently used as Services don't have a Dispose event
        /// </summary>
        public void Dispose()
        {
        }

        #region Paging Methods


        /// <summary>
        /// List of the records to display
        /// </summary>
        public List<T> PagedRecords { get; set; }

        /// <summary>
        /// current page being Displayed
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Current group start page
        /// </summary>
        public int StartPage { get; set; }

        /// <summary>
        /// Current Group end Page
        /// </summary>
        public int EndPage { get; set; }

        /// <summary>
        /// No of records to display on a page
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Size of the page block - i.e. how many pages are displayed in the paging control when we have large numbers of pages 
        /// </summary>
        public int PagingBlockSize { get; set; } = 10;

        /// <summary>
        /// Current Sorting Collumn
        /// </summary>
        public string SortColumn { get; set; }

        /// <summary>
        /// Default sorting column when reloading
        /// </summary>
        public string DefaultSortColumn { get; set; }

        /// <summary>
        /// Bool to tell the control not to paginate
        /// </summary>
        public bool NoPagination { get; set; }

        /// <summary>
        /// Srting direction for the sort column
        /// </summary>
        public SortDirection SortingDirection { get; set; }

        /// <summary>
        /// Default sorting direction
        /// useful when default is date and you want to show latest date at the top
        /// </summary>
        public SortDirection DefaultSortingDirection { get; set; } = SortDirection.Ascending;

        /// <summary>
        /// Start record no for the current page
        /// </summary>
        public int PageStartRecord => this._CurPage * this._PageSize;

        /// <summary>
        /// size of the page to fetch
        /// normally ther page size but for the last page may be different
        /// </summary>
        public int PageGetSize => this.RecordCount - this.PageStartRecord < 25 ? this.RecordCount - this.PageStartRecord : this._PageSize;

        /// <summary>
        /// Total number of pages in the full dataset
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(this.RecordCount / (decimal)this._PageSize);

        /// <summary>
        /// Bool to check if there are enough records to paginate
        /// </summary>
        public bool IsPagination => (this.TotalPages > 1);

        /// <summary>
        /// Bool to tell us if we have any records
        /// </summary>
        public bool HasPagedRecords => this.PagedRecords != null && this.PagedRecords.Count > 0;

        /// <summary>
        /// Internal property to set the current page for the PageStartRecord to use
        /// </summary>
        private int _CurPage => this.CurrentPage > 1 ? this.CurrentPage - 1 : 0;

        /// <summary>
        /// Internal property used by the paging methods
        /// </summary>
        protected int _PageSize => this.NoPagination ? this.RecordCount : this.PageSize;

        /// <summary>
        /// Event triggered when the page has changed
        /// principly used by the Paging Control when an external event has reloaded the dataset and forced a paging reset
        /// e.g. pages that use filter controls
        /// </summary>
        public event EventHandler<int> PageHasChanged;

        /// <summary>
        /// Delegate for the page loaders
        /// Methods need to conform to the pattern Method(PaginationData<T> param)
        /// </summary>
        public IControllerPagingService<T>.PageLoaderDelegateAsync PageLoaderAsync { get; set; }

        /// <summary>
        /// Moves forward or backwards one block
        /// direction 1 for forwards
        /// direction -1 for backwards
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="suppresspageupdate"></param>
        public void ChangeBlock(int direction, bool suppresspageupdate = true)
        {
            if (direction == 1 && this.EndPage < this.TotalPages)
            {
                this.StartPage = this.EndPage + 1;
                if (this.EndPage + this.PagingBlockSize < this.TotalPages) this.EndPage = this.StartPage + this.PagingBlockSize - 1;
                else this.EndPage = this.TotalPages;
                if (!suppresspageupdate) this.CurrentPage = this.StartPage;
            }
            else if (direction == -1 && this.StartPage > 1)
            {
                this.EndPage = this.StartPage - 1;
                this.StartPage = this.StartPage - this.PagingBlockSize;
                if (!suppresspageupdate) this.CurrentPage = this.StartPage;
            }
            else if (direction == 0 && this.CurrentPage == 1)
            {
                this.StartPage = 1;
                if (this.EndPage + this.PagingBlockSize < this.TotalPages) this.EndPage = this.StartPage + this.PagingBlockSize - 1;
                else this.EndPage = this.TotalPages;
            }
            if (!suppresspageupdate) this.Paginate();
        }

        /// <summary>
        /// Moves forward or backwards one page
        /// direction 1 for forwards
        /// direction -1 for backwards
        /// </summary>
        /// <param name="direction"></param>
        public void MoveOnePage(int direction)
        {
            if (direction == 1)
            {
                if (this.CurrentPage < this.TotalPages)
                {
                    if (this.CurrentPage == this.EndPage) ChangeBlock(1);
                    this.CurrentPage += 1;
                }
            }
            else if (direction == -1)
            {
                if (this.CurrentPage > 1)
                {
                    if (this.CurrentPage == this.StartPage) ChangeBlock(-1);
                    this.CurrentPage -= 1;
                }
            }
            this.Paginate();
        }

        /// <summary>
        /// Moves to the Specified page
        /// </summary>
        /// <param name="pageno"></param>
        public void GoToPage(int pageno)
        {
            this.CurrentPage = pageno;
            this.Paginate();
        }

        /// <summary>
        /// Event handler that triggers a reload.
        /// Normally wired to the Service ListHasChanged Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void ReloadAsync(object sender, EventArgs e)
        {
            await LoadAsync();
        }

        /// <summary>
        /// Async Method to reload pagination.  Normally called by an external event when fitering is applied to the dataset
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LoadAsync()
        {
            this.CurrentPage = 1;
            if (!string.IsNullOrEmpty(this.DefaultSortColumn)) this.SortColumn = this.DefaultSortColumn;
            this.SortingDirection = DefaultSortingDirection;
            if (this.PageLoaderAsync != null)
            {
                this.PagedRecords = await this.PageLoaderAsync();
            }
            this.ChangeBlock(0);
            this.PageHasChanged?.Invoke(this, this.CurrentPage);
            return true;
        }

        /// <summary>
        /// Method to trigger the page Changed Event
        /// </summary>
        public void Paginate()
        {
            if (this.PageLoaderAsync != null) this.PaginateAsync().Wait();
        }

        /// <summary>
        /// Method to trigger the page Changed Event
        /// </summary>
        public async Task PaginateAsync()
        {
            if (this.PageLoaderAsync != null) this.PagedRecords = await this.PageLoaderAsync();
            this.PageHasChanged?.Invoke(this, this.CurrentPage);
        }

        /// <summary>
        /// Handler for a column sort event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="columnname"></param>
        public void Sort(object sender, string columnname)
        {
            this.SortColumn = columnname;
            if (this.SortingDirection == SortDirection.Ascending) SortingDirection = SortDirection.Descending;
            else SortingDirection = SortDirection.Ascending;
            this.GoToPage(1);
        }

        /// <summary>
        /// Method to get the correct icon to display in the column title to show the sorting state for the column
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string GetIcon(string columnName)
        {
            if (this.SortColumn != columnName) return "sort-column oi oi-resize-height";
            if (this.SortingDirection == SortDirection.Ascending) return "sort-column oi oi-sort-descending";
            else return "sort-column oi oi-sort-ascending";
        }

        #endregion

    }
}
