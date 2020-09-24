using CEC.Blazor.Components;
using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Services
{
    public abstract class BaseControllerService<TRecord, TContext> :
        IDisposable,
        IControllerPagingService<TRecord>,
        IControllerService<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext

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
        public IDataService<TRecord, TContext> Service { get; set; }

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
        /// The record Configuration - read from the data service
        /// </summary>
        public RecordConfigurationData RecordConfiguration => this.Service?.RecordConfiguration ?? new RecordConfigurationData();

        /// <summary>
        /// The Current Record
        /// Logic ensures a record always exists and the Shadow copy is up to date
        /// </summary>
        public TRecord Record
        {
            get
            {
                this._Record ??= new TRecord();
                return this._Record;
            }

            set => this._Record = value;
        }

        /// <summary>
        /// The actual Current Record
        /// </summary>
        protected TRecord _Record { get; private set; }

        /// <summary>
        /// Shadow Copy of the Current Record
        /// Should always be an unaltered copy of what's in the database
        /// Only used by Editors
        /// </summary>
        public TRecord ShadowRecord { set; get; } = new TRecord();

        /// <summary>
        /// Public Current List of Records used for Listing Operations
        /// </summary>
        public List<TRecord> Records
        {
            get => this._Records;
            set => this._Records = value;
        }

        /// <summary>
        /// The Actual Current List of Records
        /// </summary>
        public List<TRecord> _Records { get; private set; }

        /// <summary>
        /// Property to expose the Record ID
        /// Implemented in inherited classes with error checking for Record Exists
        /// </summary>
        public virtual int RecordID => this.Record?.ID ?? -1;

        /// <summary>
        /// Boolean Property to check if a real record exists 
        /// </summary>
        public virtual bool IsRecord => this.RecordID > -1;

        /// <summary>
        /// Boolean Property to check if a New record exists 
        /// </summary>
        public virtual bool IsNewRecord => this.RecordID == 0;

        /// <summary>
        /// Property exposing the number of records in the current list
        /// </summary>
        public int RecordCount => this.Records?.Count ?? -1;


        /// <summary>
        /// Property to hold the record count by direct database query
        /// </summary>
        public int BaseRecordCount { get; protected set; } = 0;

        /// <summary>
        /// Boolean Property used to check if the record list exists
        /// </summary>
        public bool IsRecords => this.RecordCount >= 0;

        /// <summary>
        /// Boolean Property used to check if the Data Service is set
        /// </summary>
        public bool IsService => this.Service != null;

        /// <summary>
        /// Property exposing the current save state of the record 
        /// </summary>
        public bool IsClean { get; protected set; } = true;

        /// <summary>
        /// Used by the list methods to filter the list contents.
        /// </summary>
        public virtual IFilterList FilterList { get; set; } = new FilterList();

        #endregion

        #region protected/private properties

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

        /// <summary>
        /// Event triggered whwen the list has changed
        /// </summary>
        public event EventHandler FilterHasChanged;

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
        public async virtual Task Reset()
        {
            this.FilterList = new FilterList();
            this.Record = new TRecord();
            this.ShadowRecord = new TRecord();
            this.Records = new List<TRecord>();
            this.PagedRecords = null;
            this.BaseRecordCount = await this.Service.GetRecordListCountAsync();
            this.SetClean();
        }

        /// <summary>
        /// Method to trigger a Record Changed Event
        /// </summary>
        public virtual void TriggerFilterChangedEvent(object sender) => this.FilterHasChanged?.Invoke(sender, EventArgs.Empty);

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
        public async void ResetList(object sender, EventArgs e)
        {
            Records = new List<TRecord>();
            this.BaseRecordCount = await this.Service.GetRecordListCountAsync();
            this.TriggerListChangedEvent(sender);
        }

        /// <summary>
        /// Reset the List to null  We set to null because:
        /// A null list means the list needs reloading.  It's how the Paging system knows to reload the list
        /// An empty list may be a no results returned list
        /// </summary>
        public async virtual Task ResetList()
        {
            this.Records = null;
            this.BaseRecordCount = await this.Service.GetRecordListCountAsync();
            this.TriggerListChangedEvent(this);
        }

        /// <summary>
        /// Async Reset the List to null  We set to null because:
        /// A null list means the list needs reloading.  It's how the Paging system knows to reload the list
        /// An empty list may be a no results returned list
        /// </summary>
        public async virtual Task ResetListAsync()
        {
            this.Records = null;
            this.BaseRecordCount = await this.Service.GetRecordListCountAsync();
            this.TriggerListChangedEvent(this);
        }

        public virtual Task<bool> GetNewRecordAsync()
        {
            this.Record = new TRecord();
            this.ShadowRecord = new TRecord();
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
        public async virtual Task<List<TRecord>> GetDataPageAsync()
        {
            await this.GetFilteredListAsync();
            if (this.PageStartRecord < this.Records.Count) return this.Records.GetRange(this.PageStartRecord, this.PageGetSize);
            return new List<TRecord>();
        }

        /// <summary>
        /// Base Implementation of the sorted version of paging.  Should be onveridden in descendants
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        public async virtual Task<List<TRecord>> GetDataPageWithSortingAsync()
        {
            // Get the filtered list - will only get a new list if the Records property has been set to null elsewhere
            await this.GetFilteredListAsync();
            // Reset the start record if we are outside the range of the record set - a belt and braces check as this shouldn't happen!
            if (this.PageStartRecord > this.Records.Count) this.CurrentPage = 1;
            // Check if we have to apply sorting, in not get the page we want
            if (string.IsNullOrEmpty(this.SortColumn)) return this.Records.Skip(this.PageStartRecord).Take(this._PageSize).ToList();
            else
            {
                //  If we do order the record set and then get the page we want
                if (this.SortingDirection == SortDirection.Ascending)
                {
                    return this.Records.OrderBy(x => x.GetType().GetProperty(this.SortColumn).GetValue(x, null)).Skip(this.PageStartRecord).Take(this._PageSize).ToList();
                }
                else
                {
                    return this.Records.OrderByDescending(x => x.GetType().GetProperty(this.SortColumn).GetValue(x, null)).Skip(this.PageStartRecord).Take(this._PageSize).ToList();
                }
            }
        }

        /// <summary>
        /// Base implementation that gets the record list based on the FilterList.  Override for specific filtering
        /// Set the record List to null to force a reload
        /// </summary>
        public async virtual Task<bool> GetFilteredListAsync()
        {
            // Check if the record set is null. and only refresh the record set if it's null
            if (!this.IsRecords)
            {
                if (this.FilterList.Load) this.Records = await this.Service.GetFilteredRecordListAsync(FilterList);
                else this.Records = new List<TRecord>();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Async Gets a record from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task<bool> GetRecordAsync(int? id, bool refresh = false)
        {
            if (this.IsService)
            {
                if (id != this.RecordID || refresh)
                {
                    if (id == 0)
                    {
                        this.Record = new TRecord();
                        this.Record.SetNew();
                    }
                    else this.Record = await this.Service.GetRecordAsync(id ?? 0);
                    this.Record ??= new TRecord();
                    this.ShadowRecord = this.Record.ShadowCopy();
                    if (!this.IsRecords) this.BaseRecordCount = await this.Service.GetRecordListCountAsync();
                    this.SetClean();
                    this.TriggerRecordChangedEvent(this);
                }
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
            this.TaskResult = await this.Service.CreateRecordAsync(this.Record);
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
            if (withDelegate) this.PageLoaderAsync = new IControllerPagingService<TRecord>.PageLoaderDelegateAsync(this.GetDataPageWithSortingAsync);
            // loads the paging object
            await this.LoadAsync();
            // Trigger event so any listeners get notified
            this.TriggerListChangedEvent(this);
        }

        /// <summary>
        /// Method to get a lookup list of values for TLookup record
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public async Task<SortedDictionary<int, string>> GetLookUpListAsync<TLookup>(string selectAllText = null) where TLookup : class, IDbRecord<TLookup>
        {
            var list = await this.Service.GetBaseRecordListAsync<TLookup>();
            var LookupList = new SortedDictionary<int, string>();
            if (!string.IsNullOrEmpty(selectAllText)) LookupList.Add(0, selectAllText);
            list.ForEach(item => LookupList.Add(item.ID, item.DisplayName));
            return LookupList;
        }

        /// <summary>
        /// Method to get a lookup list of values for a Field in TLookup record
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public async Task<List<string>> GetDistinctListAsync(DbDistinctRequest req) => await this.Service.GetDistinctListAsync(req);

        /// <summary>
        /// Method to get a base record set from an IDbRecord record
        /// </summary>
        /// <returns></returns>
        public async Task<List<DbBaseRecord>> GetBaseRecordListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup> => await this.Service.GetBaseRecordListAsync<TLookup>();

        /// <summary>
        /// Pseudo Dispose Event - not currently used as Services don't have a Dispose event
        /// </summary>
        public void Dispose()
        {
        }

        #region Paging Methods

        /// <summary>
        /// Property that gets the page size to use in Paging operations
        /// Reads it from the application configuration file
        /// </summary>
        public int DefaultPageSize => int.TryParse(this.AppConfiguration["Paging:PageSize"], out int value) ? value : 20;

        /// <summary>
        /// List of the records to display
        /// </summary>
        public List<TRecord> PagedRecords { get; set; }

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
        /// Bool to tell us if we have any records
        /// </summary>
        public bool HasNoPagedRecords => this.PagedRecords != null && this.PagedRecords.Count == 0;

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
        public IControllerPagingService<TRecord>.PageLoaderDelegateAsync PageLoaderAsync { get; set; }

        /// <summary>
        /// Moves forward or backwards one block
        /// direction 1 for forwards
        /// direction -1 for backwards
        /// suppresspageupdate 
        ///  - set to true (default) when user changes page and the block changes with the page
        ///  - set to false when user changes block rather than changing page and the page needs to be updated to the first page of the block
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="suppresspageupdate"></param>
        public async Task ChangeBlockAsync(int direction, bool suppresspageupdate = true)
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
            if (!suppresspageupdate) await this.PaginateAsync();
        }

        /// <summary>
        /// Moves forward or backwards one page
        /// direction 1 for forwards
        /// direction -1 for backwards
        /// </summary>
        /// <param name="direction"></param>
        public async Task MoveOnePageAsync(int direction)
        {
            if (direction == 1)
            {
                if (this.CurrentPage < this.TotalPages)
                {
                    if (this.CurrentPage == this.EndPage) await ChangeBlockAsync(1);
                    this.CurrentPage += 1;
                }
            }
            else if (direction == -1)
            {
                if (this.CurrentPage > 1)
                {
                    if (this.CurrentPage == this.StartPage) await ChangeBlockAsync(-1);
                    this.CurrentPage -= 1;
                }
            }
            await this.PaginateAsync();
        }

        /// <summary>
        /// Moves to the Specified page
        /// </summary>
        /// <param name="pageno"></param>
        public async Task GoToPageAsync(int pageno)
        {
            this.CurrentPage = pageno;
            await this.PaginateAsync();
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
            // Reset the page to 1
            this.CurrentPage = 1;
            // Check if we have a sort column, if not set to the default column
            if (!string.IsNullOrEmpty(this.DefaultSortColumn)) this.SortColumn = this.DefaultSortColumn;
            // Set the sort direction to the default
            this.SortingDirection = DefaultSortingDirection;
            // Check if we have a method loaded in the PageLoaderAsync delegate and if so run it
            if (this.PageLoaderAsync != null) this.PagedRecords = await this.PageLoaderAsync();
            // Set the block back to the start
            await this.ChangeBlockAsync(0);
            //  Force a UI update as everything has changed
            this.PageHasChanged?.Invoke(this, this.CurrentPage);
            return true;
        }

        /// <summary>
        /// Method to trigger the page Changed Event
        /// </summary>
        public async Task PaginateAsync()
        {
            // Check if we have a method loaded in the PageLoaderAsync delegate and if so run it
            if (this.PageLoaderAsync != null) this.PagedRecords = await this.PageLoaderAsync();
            //  Force a UI update as something has changed
            this.PageHasChanged?.Invoke(this, this.CurrentPage);
        }

        /// <summary>
        /// Handler for a column sort event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="columnname"></param>
        public async void Sort(object sender, string columnname)
        {
            this.SortColumn = columnname;
            if (this.SortingDirection == SortDirection.Ascending) SortingDirection = SortDirection.Descending;
            else SortingDirection = SortDirection.Ascending;
            await this.GoToPageAsync(1);
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
