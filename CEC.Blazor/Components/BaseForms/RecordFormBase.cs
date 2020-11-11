using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
namespace CEC.Blazor.Components.BaseForms
{
    public class RecordFormBase<TRecord, TContext> : 
        ControllerServiceFormBase<TRecord, TContext> 
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
        protected virtual bool IsRecord => this.Service?.IsRecord ?? false;

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
        /// Property for the ID of the record to retrieve.
        /// Normally set by Routing e.g. /Farm/Edit/1
        /// </summary>
        [Parameter]
        public int? ID
        {
            get => this._ID;
            set => this._ID = (value is null) ? -1 : (int)value;
        }

        /// <summary>
        /// Version of the ID that sets null to 0
        /// </summary>
        public int _ID { get; private set; }

        protected async override Task OnRenderAsync(bool firstRender)
        {
            if (firstRender && this.IsService) await this.Service.ResetRecordAsync();
            await this.LoadRecordAsync(firstRender);
            await base.OnRenderAsync(firstRender);
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return base.OnAfterRenderAsync(firstRender);
        }

        /// <summary>
        /// Reloads the record if the ID has changed
        /// </summary>
        /// <returns></returns>
        protected virtual async Task LoadRecordAsync(bool firstload = false)
        {
            if (this.IsService)
            {
                // Set the Loading flag 
                this.IsDataLoading = true;
                //  call StateHasChanged only if we are responding to an event.  In the component loading cycle it will be called for us shortly
                if (!firstload) await RenderAsync();
                if (this.IsModal && this.ViewManager.ModalDialog.Options.Parameters.TryGetValue("ID", out object modalid)) this.ID = (int)modalid > -1 ? (int)modalid : this.ID;

                // Get the current record - this will check if the id is different from the current record and only update if it's changed
                await this.Service.GetRecordAsync(this._ID, false);

                // Set the error message - it will only be displayed if we have an error
                this.RecordErrorMessage = $"The Application can't load the Record with ID: {this._ID}";

                // Set the Loading flag
                this.IsDataLoading = false;
                //  call Render only if we are responding to an event.  In the component loading cycle it will be called for us shortly
                if (!firstload) await RenderAsync();
            }
        }
    }
}
