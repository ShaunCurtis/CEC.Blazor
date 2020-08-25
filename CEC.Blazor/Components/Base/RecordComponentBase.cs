using CEC.Blazor.Components.UIControls;
using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
using System;
using System.Threading.Tasks;
namespace CEC.Blazor.Components.Base
{
    public class RecordComponentBase<T> : ControllerServiceComponentBase<T> where T : IDbRecord<T>, new()
    {
        /// <summary>
        /// This Page/Component Title
        /// </summary>
        public virtual string PageTitle { get; }

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
            await this.LoadRecord();
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                this.RouterSessionService.IntraPageNavigation += this.OnIntraPageRouting;
            }
        }

        /// <summary>
        /// Reloads the record if the ID has changed
        /// </summary>
        /// <returns></returns>
        protected virtual async Task LoadRecord()
        {
            if (this.IsService)
            {
                this.IsDataLoading = true;
                StateHasChanged();
                // Check if we have a query string value for id and if so use it
                if (this.NavManager.TryGetQueryString<int>("id", out int querystringid)) this.ID = querystringid > 0 ? querystringid : this._ID;
                // Check if the component is a modal and if so get the supplied ID
                else if (this.IsModal && this.Parent.Options.Parameters.TryGetValue("ID", out object modalid)) this.ID = (int)modalid > 0 ? (int)modalid : this.ID;

                await Task.Delay(500);
                // Get the current ID record if the id is different from the current record
                await this.Service.GetRecordAsync(this._ID, false);

                //await this.Service.ResetRecordAsync();

                this.RecordErrorMessage = $"The Application can't load the Record with ID: {this._ID}";
                this.IsDataLoading = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Event triggered when we have IntraPage routing - in our case we are lokking for querystring changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void OnIntraPageRouting(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"RecordComponentBase IntraPage Navigation Triggered");
            await LoadRecord();
        }
    }
}
