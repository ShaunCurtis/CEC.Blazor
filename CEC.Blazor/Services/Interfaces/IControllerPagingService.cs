using CEC.Blazor.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Blazor.Services
{
    /// <summary>
    /// Interface for the Paging Component
    /// </summary>
    public interface IControllerPagingService<TRecord> where TRecord : IDbRecord<TRecord>, new()
    {
        /// <summary>
        /// List of the records to display
        /// </summary>
        public List<TRecord> PagedRecords { get; set; }

        /// <summary>
        /// current page being Displayed
        /// </summary>
        public int CurrentPage { get; set; }

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
        public int PageSize { get; set; }

        /// <summary>
        /// Size of the page block - i.e. how many pages are displayed in the paging control when we have large numbers of pages 
        /// </summary>
        public int PagingBlockSize { get; set; }

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
        public SortDirection DefaultSortingDirection { get; set; }

        /// <summary>
        /// Start record no for the current page
        /// </summary>
        public int PageStartRecord { get; }

        /// <summary>
        /// size of the page to fetch
        /// normally ther page size but for the last page may be different
        /// </summary>
        public int PageGetSize { get; }

        /// <summary>
        /// Total number of pages in the full dataset
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// Bool to check if there are enough records to paginate
        /// </summary>
        public bool IsPagination { get; }

        /// <summary>
        /// Bool to tell us if we have any records
        /// </summary>
        public bool HasPagedRecords { get; }

        /// <summary>
        /// Bool to tell us if we have no records
        /// Note this is different from records set to null
        /// </summary>
        public bool HasNoPagedRecords { get; }


        // Event that is triggered when a page update occurs
        /// <summary>
        /// Event triggered when the page has changed
        /// principly used by the Paging Control when an external event has reloaded the dataset and forced a paging reset
        /// e.g. pages that use filter controls
        /// </summary>
        public event EventHandler<int> PageHasChanged;

        /// <summary>
        /// Delegate definition for the Page Loader
        /// Methods need to conform to the pattern Method(PaginationData<TRecord> param)
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public delegate Task<List<TRecord>> PageLoaderDelegateAsync();

        /// <summary>
        /// Delegate for the page loaders
        /// Methods need to conform to the pattern Method(PaginationData<TRecord> param)
        /// </summary>
        public PageLoaderDelegateAsync PageLoaderAsync { get; set; }

        /// <summary>
        /// Moves forward or backwards one block
        /// direction 1 for forwards
        /// direction -1 for backwards
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="suppresspageupdate"></param>
        public Task ChangeBlockAsync(int direction, bool suppresspageupdate = true);

        /// <summary>
        /// Moves forward or backwards one page
        /// direction 1 for forwards
        /// direction -1 for backwards
        /// </summary>
        /// <param name="direction"></param>
        public Task MoveOnePageAsync(int direction);

        /// <summary>
        /// Moves to the Specified page
        /// </summary>
        /// <param name="pageno"></param>
        public Task GoToPageAsync(int pageno);

        /// <summary>
        /// Event handler that triggers a reload.
        /// Normally wired to the Service ListHasChanged Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReloadAsync(object sender, EventArgs e);

        /// <summary>
        /// Async Method to reload pagination.  Normally called by an external event when fitering is applied to the dataset
        /// </summary>
        /// <returns></returns>
        public Task<bool> LoadAsync(int page = 1);

        /// <summary>
        /// Method to trigger the page Changed Event
        /// </summary>
        public Task PaginateAsync();

        /// <summary>
        /// Handler for a column sort event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="columnname"></param>
        public void Sort(object sender, string columnname);

        /// <summary>
        /// Method to get the correct icon to display in the column title to show the sorting state for the column
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string GetIcon(string columnName);

    }
}
