using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CEC.Blazor.Services;

namespace CEC.Blazor.Components
{

    public class PagingData<T>
    {
        public enum SortDirection
        {
            Ascending,
            Descending
        }

        public IPagingService PagingService { get; set; }

        /// <summary>
        /// List of the records to display
        /// </summary>
        public List<T> Records { get; private set; }

        /// <summary>
        /// current page being Displayed
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Size of the page block - i.e. how many pages are displayed in the paging control when we have large numbers of pages 
        /// </summary>
        public int BlockSize { get; set; } = 10;

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
        public int PageSize { get; private set; }

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
        public SortDirection SortingDirection { get; set; } = SortDirection.Ascending;

        /// <summary>
        /// Default sorting direction
        /// useful when default is date and you want to show latest date at the top
        /// </summary>
        public SortDirection DefaultSortingDirection { get; set; } = SortDirection.Ascending;

        /// <summary>
        /// Start record no for the current page
        /// </summary>
        public int PageStartRecord { get => this.CurPage * PageSize; }

        /// <summary>
        /// size of the page to fetch
        /// normally ther page size but for the last page may be different
        /// </summary>
        public int PageGetSize { get => this.DataSetCount - this.PageStartRecord < 25 ? this.DataSetCount - this.PageStartRecord : PageSize; }

        /// <summary>
        /// Total Number of Records in the data set
        /// </summary>
        public int DataSetCount { get; private set; }

        /// <summary>
        /// Total number of pages in the full dataset
        /// </summary>
        public int TotalPages { get => (int)Math.Ceiling(this.DataSetCount / (decimal)this.PageSize); }

        /// <summary>
        /// Bool to check if there are enough records to paginate
        /// </summary>
        public bool IsPagination { get => (this.TotalPages > 1); }

        /// <summary>
        /// Bool to tell us if we have any records
        /// </summary>
        public bool HasRecords { get => this.Records != null && this.Records.Count > 0; }

        /// <summary>
        /// Internal property to set the current page for the PageStartRecord to use
        /// </summary>
        private int CurPage { get => this.CurrentPage > 1 ? this.CurrentPage - 1 : 0; }

        // Event that is triggered when a page update occurs
        /// <summary>
        /// Event triggered when the page has changed
        /// principly used by the Paging Control when an external event has reloaded the dataset and forced a paging reset
        /// e.g. pages that use filter controls
        /// </summary>
        public event EventHandler<int> PageHasChanged;

        /// <summary>
        /// Delegate definition for the Page Loader
        /// Methods need to conform to the pattern Method(PaginationData<T> param)
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public delegate Task<List<T>> PageLoaderDelegateAsync(PagingData<T> param);

        /// <summary>
        /// Delegate for the page loaders
        /// Methods need to conform to the pattern Method(PaginationData<T> param)
        /// </summary>
        public PageLoaderDelegateAsync PageLoaderAsync { get; set; }

        /// <summary>
        /// Class Initialization
        /// we don't allow an empty class because we need to know the page size and record count to start
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="recordcount"></param>
        public PagingData(int pagesize, int recordcount)
        {
            this.DataSetCount = recordcount;
            this.PageSize = pagesize;
            if (recordcount > 0)
            {
                if (this.NoPagination) this.PageSize = recordcount;
                this.ChangeBlock(1);
                this.CurrentPage = 1;
                if (!string.IsNullOrEmpty(this.DefaultSortColumn)) this.SortColumn = this.DefaultSortColumn;
                this.SortingDirection = DefaultSortingDirection;
            }
            this.Paginate();
        }

        /// <summary>
        /// Class Initialization
        /// we don't allow an empty class because we need to know the page size and record count to start
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="recordcount"></param>
        /// <param name="pagingService"></param>
        public PagingData(int pagesize, int recordcount, IPagingService pagingService)
        {
            this.DataSetCount = recordcount;
            this.PageSize = pagesize;
            this.PagingService = pagingService;
            if (recordcount > 0)
            {
                if (this.NoPagination) this.PageSize = recordcount;
                this.ChangeBlock(1);
                this.CurrentPage = 1;
                if (!string.IsNullOrEmpty(this.DefaultSortColumn)) this.SortColumn = this.DefaultSortColumn;
                this.SortingDirection = DefaultSortingDirection;
            }
            pagingService.ListHasChanged += this.ReloadAsync;
        }

        public void Dispose()
        {
            if (this.PagingService != null) PagingService.ListHasChanged -= this.ReloadAsync;

        }

        /// <summary>
        /// Method to reset the record count of the dataset
        /// </summary>
        /// <param name="recordcount"></param>
        public void ResetRecordCount(int recordcount)
        {
            this.DataSetCount = recordcount;
            if (recordcount > 0)
            {
                if (this.NoPagination) this.PageSize = recordcount;
                this.CurrentPage = 1;
                this.ChangeBlock(0);
            }
            else this.Records = new List<T>();
        }

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
                if (this.EndPage + this.BlockSize < this.TotalPages) this.EndPage = this.StartPage + this.BlockSize - 1;
                else this.EndPage = this.TotalPages;
                if (!suppresspageupdate) this.CurrentPage = this.StartPage;
            }
            else if (direction == -1 && this.StartPage > 1)
            {
                this.EndPage = this.StartPage - 1;
                this.StartPage = this.StartPage - this.BlockSize;
                if (!suppresspageupdate) this.CurrentPage = this.StartPage;
            }
            else if (direction == 0 && this.CurrentPage == 1)
            {
                this.StartPage = 1;
                if (this.EndPage + this.BlockSize < this.TotalPages) this.EndPage = this.StartPage + this.BlockSize - 1;
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
                this.Records = await this.PageLoaderAsync(this);
            }
            this.PageHasChanged?.Invoke(this, this.CurrentPage);
            return true;
        }

        /// <summary>
        /// Method to trigger the page Changed Event
        /// </summary>
        public void Paginate()
        {
            if (this.PageLoaderAsync != null)
            {
                var task = this.PaginateAsync();
                task.Wait();
            }
        }

        /// <summary>
        /// Method to trigger the page Changed Event
        /// </summary>
        public async Task PaginateAsync()
        {
            if (this.PageLoaderAsync != null) this.Records = await this.PageLoaderAsync(this);
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
    }
}
