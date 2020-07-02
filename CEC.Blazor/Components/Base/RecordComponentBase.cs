using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
using System;
using System.Runtime.CompilerServices;
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
        /// Version of the ID that sets null to 0
        /// </summary>
        public int CurrentID { get; protected set; }

        /// <summary>
        /// Boolean Property that checks if a record exists
        /// </summary>
        protected virtual bool IsRecord => this.IsService && this.Service.IsRecord;

        /// <summary>
        /// Used to determine if the page can display data
        /// </summary>
        protected virtual bool IsError { get => !this.IsRecord; }

        /// <summary>
        /// Should be overriddebn in inherited components
        /// and called after setting the Service
        /// </summary>
        protected async override Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected async override Task LoadAsync()
        {
            await base.LoadAsync();
            if (this.IsService)
            {
                // Check if we have a query string value and use it if we do
                this.NavManager.TryGetQueryString<int>("id", out int id);
                this.ID = id > 0 ? id : this._ID;
                this.CurrentID = this._ID;
                // Get the current ID record
                await this.Service.GetRecordAsync(this._ID);
                this.Service.RecordHasChanged += this.UpdateState;
                this.RouterSessionService.IntraPageNavigation += this.OnIntraPageNavigation;
            }
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (this.IsError)
            {
                this.RecordErrorMessage = "The Application can't load the Record";
                this.UpdateState();
            }
        }

        protected async void OnIntraPageNavigation(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"RecordComponentBase IntraPage Navigation Triggered");
            // Check if we have a query string value and use it if we do
            this.NavManager.TryGetQueryString<int>("id", out int id);
            this.ID = id > 0 ? id : this._ID;
            // Check if it's changed and if so reload the record
            if (this.CurrentID != this._ID)
            {
                await this.Service.GetRecordAsync(this._ID);
                this.CurrentID = this._ID;
            }
        }
    }
}
