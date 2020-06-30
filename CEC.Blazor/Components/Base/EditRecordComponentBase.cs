using CEC.Blazor.Data;
using CEC.Routing.Components;
using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.Base
{
    public class EditRecordComponentBase<T> : RecordComponentBase<T>, IRecordRoutingComponent where T : IDbRecord<T>, new()
    {
        /// <summary>
        /// This Page URL/route
        /// </summary>
        public string PageUrl { get; set; }

        /// <summary>
        /// Boolean Property controlling Routing
        /// </summary>
        public bool IsClean => this.Service?.IsClean ?? true;

        /// <summary>
        /// Boolean Property set when a navigation event has been cancelled
        /// </summary>
        public bool NavigationCancelled { get; set; }

        /// <summary>
        /// Boolean check if the record is New or Existing
        /// </summary>
        protected virtual bool IsNewRecord { get => this._ID < 1; }

        /// <summary>
        /// EditContext for the component
        /// </summary>
        protected EditContext EditContext { get; set; }

        /// <summary>
        /// Should be overriddebn in inherited components
        /// and called after setting the Service
        /// </summary>
        protected async override Task OnInitializedAsync()
        {
            this.PageUrl = this.NavManager.Uri;
            this.RouterSessionService.ActiveComponent = this;
            this.RouterSessionService.NavigationCancelled += this.OnNavigationCancelled;
            this.EditContext = new EditContext(this.Service.Record);
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Event handler for a navigsation cancelled event raised by the router
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnNavigationCancelled(object sender, EventArgs e)
        {
            this.NavigationCancelled = true;
            this.AlertMessage.SetAlert("<b>RECORD ISN'T SAVED</b>. Either Save or Exit Without Saving.", Alert.AlertDanger);
            this.StateHasChanged();
        }

        /// <summary>
        /// Event handler for the RecordFromControls FieldChanged Event
        /// </summary>
        /// <param name="changestate"></param>
        protected virtual void RecordFieldChanged(bool changestate)
        {
            if (this.EditContext != null)
            {
                this.Service.SetClean(!this.EditContext.IsModified());
                if (this.IsClean) this.AlertMessage.ClearAlert();
                else this.AlertMessage.SetAlert("The Record is not Saved", Alert.AlertWarning);
                this.UpdateUI();
                this.StateHasChanged();
            }
        }

        protected virtual Task UpdateUI()
        {
            return Task.CompletedTask;
        }
    }
}
