using CEC.Blazor.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Services
{
    public interface IControllerService<T> where T : IDbRecord<T>, new()
    {
        /// <summary>
        /// Unique ID for the Service
        /// Helps in debugging
        /// </summary>
        public Guid ID { get; set; }

        public IDataService<T> Service { get; set; }

        /// <summary>
        /// Record Configuration Object - provides name and routing information for the DB Model Class
        /// </summary>
        public RecordConfigurationData RecordConfiguration { get; }

        /// <summary>
        /// Current Record - for CRUD Component Operations
        /// </summary>
        public T Record { get; set; }

        /// <summary>
        /// Shadow Copy of Current Record - for CRUD Component Operations
        /// </summary>
        public T ShadowRecord { get; set; }

        /// <summary>
        /// Current List of Records - used in Listing Components 
        /// </summary>
        public List<T> Records { get; set; }

        /// <summary>
        /// Property to get the default page size retrieved from the application Configuration
        /// </summary>
        public int DefaultPageSize { get; }

        /// <summary>
        /// Property to get current record count
        /// </summary>
        public int RecordCount => this.IsRecord ? this.Records.Count : 0;

        /// <summary>
        /// Boolean Property to check if a record exists
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
        /// Property to expose the Record ID.
        /// should be implemented to return 0 if the record is null
        /// </summary>
        public abstract int RecordID { get; }

        /// <summary>
        /// Property of DbTaskResult set when a CRUD operation is called
        /// The UI can build an alert/confirmation method from the information provided
        /// </summary>
        public DbTaskResult TaskResult { get; }

        /// <summary>
        /// Property exposing the current edit state of the record 
        /// </summary>
        public bool IsClean { get; }

        /// <summary>
        /// Event raised when the Record has Changed
        /// </summary>
        public event EventHandler RecordHasChanged;

        /// <summary>
        /// Event raised when the List has Changed
        /// </summary>
        public event EventHandler ListHasChanged;

        /// <summary>
        /// Event triggered when the record is edited and not saved
        /// </summary>
        public event EventHandler OnDirty;

        /// <summary>
        /// Event triggered when the record is saved
        /// </summary>
        public event EventHandler OnClean;

        /// <summary>
        /// Method to Reset the Service to New condition
        /// </summary>
        public void Reset();

        /// <summary>
        /// Method to set the state to clean
        /// </summary>
        public void SetClean(bool isClean = true);

        /// <summary>
        /// Method to set the state to clean
        /// </summary>
        public void SetDirty();

        /// <summary>
        /// Method to Update or Add the Database Record
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveRecordAsync() {
            if (this.Record.ID > 0) return await UpdateRecordAsync();
            else return await AddRecordAsync();
        }

        /// <summary>
        /// Method to Update the Database Record
        /// </summary>
        /// <returns></returns>
        public Task<bool> UpdateRecordAsync() => Task.FromResult(false);

        /// <summary>
        /// Method to add a Record to the Database
        /// </summary>
        /// <returns></returns>
        public Task<bool> AddRecordAsync() => Task.FromResult(false);

        /// <summary>
        /// Method to get a Record from the Database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<bool> GetRecordAsync(int? id) => Task.FromResult(false);

        /// <summary>
        /// Method to get a new Record
        /// </summary>
        /// <returns></returns>
        public Task<bool> GetNewRecordAsync() => Task.FromResult(false);

        /// <summary>
        /// Method to reset the record to new
        /// </summary>
        /// <returns></returns>
        public Task ResetRecordAsync()
        {
            this.Record = new T();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method to load up the Paged Data to display
        /// </summary>
        public Task LoadPagingAsync(bool withDelegate = true);

        /// <summary>
        /// Method to get a page of data from the database - used during paging operations
        /// </summary>
        /// <returns></returns>
        public Task<List<T>> GetDataPageAsync();

        /// <summary>
        /// Method to rest the list to empty
        /// </summary>
        /// <returns></returns>
        public Task ResetListAsync();

        /// <summary>
        /// Method to create a copy of the current record
        /// </summary>
        /// <returns></returns>
        public Task CopyRecordAsync() => Task.CompletedTask;
    }
}
