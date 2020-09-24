using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
namespace CEC.Blazor.Components.BaseForms
{
    public class RecordComponentBase<TRecord, TContext> : 
        ControllerServiceComponentBase<TRecord, TContext> 
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext
    {
        /// <summary>
        /// This Page/Component Title
        /// </summary>
        public virtual string PageTitle => (this.Service?.Record?.DisplayName ?? string.Empty).Trim();

        /// <summary>
        /// Boolean Property that checks if a record exists
        /// </summary>
        protected virtual bool IsRecord => this?.Service.IsRecord ?? false;

        /// <summary>
        /// Used to determine if the page can display data
        /// </summary>
        protected virtual bool IsError { get => !this.IsRecord; }

        /// <summary>
        /// Used to determine if the page can display data
        /// </summary>
        protected virtual bool IsDataLoading { get; set; } = true;

        /// <summary>
        /// Used to determine if the page has display data i.e. it's not loading or in error
        /// </summary>
        protected virtual bool IsLoaded => !(this.IsDataLoading) && !(this.IsError);

        /// <summary>
        /// Inherited - Always call the base method last
        /// </summary>
        protected async override Task OnInitializedAsync()
        {
            await this.Service.ResetRecordAsync();
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Inherited - Always call the base method first
        /// </summary>
        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            // Get the record if required
            await this.LoadRecordAsync();
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            // Wire up the SameComponentNavigation Event - i.e. we potentially have a new record to load in the same View 
            if (firstRender) this.RouterSessionService.SameComponentNavigation += this.OnSameRouteRouting;
        }

        /// <summary>
        /// Reloads the record if the ID has changed
        /// </summary>
        /// <returns></returns>
        protected virtual async Task LoadRecordAsync()
        {
            if (this.IsService)
            {
                // Set the Loading flag and call statehaschanged to force UI changes 
                // in this case making the UIErrorHandler show the loading spinner 
                this.IsDataLoading = true;
                StateHasChanged();

                // Check if we have a query string value in the Route for ID.  If so use it
                if (this.NavManager.TryGetQueryString<int>("id", out int querystringid)) this.ID = querystringid > -1 ? querystringid : this._ID;

                // Check if the component is a modal.  If so get the supplied ID
                else if (this.IsModal && this.Parent.Options.Parameters.TryGetValue("ID", out object modalid)) this.ID = (int)modalid > -1 ? (int)modalid : this.ID;

                // make this look slow to demo the spinner
                if (this.DemoLoadDelay > 0) await Task.Delay(this.DemoLoadDelay);

                // Get the current record - this will check if the id is different from the current record and only update if it's changed
                await this.Service.GetRecordAsync(this._ID, false);

                // Set the error message - it will only be displayed if we have an error
                this.RecordErrorMessage = $"The Application can't load the Record with ID: {this._ID}";

                // Set the Loading flag and call statehaschanged to force UI changes 
                // in this case making the UIErrorHandler show the record or the erro message 
                this.IsDataLoading = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Event triggered when we have IntraPage routing - in our case we are lokking for querystring changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void OnSameRouteRouting(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"RecordComponentBase IntraPage Navigation Triggered");
            // Gets the record - checks for a new ID in the querystring and if we have one loads the records
            await LoadRecordAsync();
        }
    }
}
