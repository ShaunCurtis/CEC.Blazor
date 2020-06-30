using CEC.Blazor.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Server.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Services
{
    public abstract class BaseControlService<T> : IControlService<T> where T : IDbRecord<T>, new()
    {
        #region Properties

        /// <summary>
        /// Unique ID for the Service
        /// Helps in debugging
        /// </summary>
        public Guid ID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Matching Data Service for T
        /// </summary>
        public IDbService<T> Service { get; set; }

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
        /// The page size to use in Paging operations
        /// </summary>
        public int DefaultPageSize => int.TryParse(this.AppConfiguration["Paging:PageSize"], out int value) ? value : 20;

        /// <summary>
        /// The record Configuration 
        /// </summary>
        public RecordConfigurationData RecordConfiguration { get; set; } = new RecordConfigurationData();

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
        public FilterList FilterList { get; set; } = new FilterList();

        /// <summary>
        /// Abstract Property to expose the Record ID
        /// Implemented in inherited classes with error checking for Record Exists
        /// </summary>
        public abstract int RecordID { get; }

        /// <summary>
        /// Boolean Property to check if a real record exists 
        /// </summary>
        public virtual bool IsRecord => this.Record != null && this.RecordID != 0;


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

        public BaseControlService(IConfiguration configuration, NavigationManager navigationManager)
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
        public async virtual Task<List<T>> GetDataPageAsync(PagingData<T> paging)
        {
            if (await this.GetFilteredListAsync()) paging.ResetRecordCount(this.Records.Count);
            if (paging.PageStartRecord < this.Records.Count) return this.Records.GetRange(paging.PageStartRecord, paging.PageGetSize);
            return new List<T>();
        }

        /// <summary>
        /// Base Implementation of the sorted version of paging.  Should be onveridden in descendants
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        public async virtual Task<List<T>> GetDataPageWithSortingAsync(PagingData<T> paging)
        {
            if (await this.GetFilteredListAsync()) paging.ResetRecordCount(this.Records.Count);
            if (paging.PageStartRecord < this.Records.Count) return this.Records.GetRange(paging.PageStartRecord, paging.PageGetSize);
            return new List<T>();
        }

        /// <summary>
        ///Base implementation that gets the full list.  Override for specific filtering
        /// </summary>
        protected async virtual Task<bool> GetFilteredListAsync()
        {
            if (!this.IsRecords)
            {
                this.TriggerListChangedEvent(this);
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
                this.ShadowRecord = ((IDbRecord<T>)this.Record).ShadowCopy();
                this.SetClean();
                this.TriggerRecordChangedEvent(this);
            }
            return this.IsRecord;
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
                this.ShadowRecord = ((IDbRecord<T>)this.Record).ShadowCopy();
                this.SetClean();
                this.TriggerRecordChangedEvent(this);
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
                this.ShadowRecord = ((IDbRecord<T>)this.Record).ShadowCopy();
                this.SetClean();
                this.TriggerRecordChangedEvent(this);
            }
            return TaskResult.IsOK && this.IsRecord;
        }

        /// <summary>
        /// Sets the Record state to Dirty/Unsaved
        /// </summary>
        /// <param name="withevents"></param>
        public virtual void SetDirty(bool withevents = true)
        {
            if (withevents) this.OnDirty?.Invoke(this, EventArgs.Empty);
            this.IsClean = false;
        }

        /// <summary>
        /// Pseudo Dispose Event - not currently used as Services don't have a Dispose event
        /// </summary>
        public void Dispose()
        {
        }

    }
}
