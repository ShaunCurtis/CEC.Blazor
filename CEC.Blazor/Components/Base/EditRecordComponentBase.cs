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
        /// EditContext for the component
        /// </summary>
        protected EditContext EditContext { get; set; }

        /// <summary>
        /// Property to concatinate the Page Title
        /// </summary>
        public override string PageTitle
        {
            get
            {
                if (this.IsNewRecord) return $"New {this.Service?.RecordConfiguration?.RecordDescription ?? "Record"}";
                else return $"{this.Service?.RecordConfiguration?.RecordDescription ?? "Record"} Editor";
            }
        }

        /// <summary>
        /// Boolean Property to determine if the record is new or an edit
        /// </summary>
        public bool IsNewRecord => this.Service?.RecordID == 0 ? true : false;

        /// <summary>
        /// Property to create the card border based on the clean state
        /// </summary>
        protected string CardBorderColour => this.IsClean ? "border-secondary" : "border-danger";

        /// <summary>
        /// Property to create the card header colour based on the clean state
        /// </summary>
        protected string CardHeaderColour => this.IsClean ? "bg-secondary text-white" : "bg-danger text-white";

        /// <summary>
        /// property used by the UIErrorHandler component
        /// </summary>
        protected override bool IsError { get => !(this.IsRecord && this.EditContext != null); }

        protected async override Task LoadAsync()
        {
            await base.LoadAsync();

            //set up the Edit Context
            this.EditContext = new EditContext(this.Service.Record);

            // Get the actual page Url from the Navigation Manager
            this.PageUrl = this.NavManager.Uri;
            // Set up the router service
            this.RouterSessionService.ActiveComponent = this;
            this.RouterSessionService.NavigationCancelled += this.OnNavigationCancelled;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
            {
            }
        }

        /// <summary>
        /// Event handler for a navigsation cancelled event raised by the router
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnNavigationCancelled(object sender, EventArgs e)
        {
            this.NavigationCancelled = true;
            this.AlertMessage.SetAlert("<b>THIS RECORD ISN'T SAVED</b>. Either <i>Save</i> or <i>Exit Without Saving</i>.", Alert.AlertDanger);
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
                if (this.IsClean)
                {
                    this.AlertMessage.ClearAlert();
                    this.BrowserService.SetExitCheck(false);
                }
                else
                {
                    this.AlertMessage.SetAlert("The Record isn't Saved", Alert.AlertWarning);
                    this.BrowserService.SetExitCheck(true);
                }
                this.UpdateUI();
                this.StateHasChanged();
            }
        }

        protected virtual Task UpdateUI()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Cancel Method called from the Button
        /// </summary>
        protected void Cancel()
        {
            this.NavigationCancelled = false;
            if (this.IsClean) this.AlertMessage.ClearAlert();
            else this.AlertMessage.SetAlert($"{this.Service.RecordConfiguration.RecordDescription} Changed", Alert.AlertWarning);
            this.UpdateState();
        }

        /// <summary>
        /// Save Method called from the Button
        /// </summary>
        protected virtual async void Save()
        {
            // Set the Shadow Copy to a copy of the current record
            // Normally run a Save/Create CRUD operation here
            await this.Service.SaveRecordAsync();
            // Set the EditContext State
            this.EditContext.MarkAsUnmodified();
            this.UpdateState();
        }

        /// <summary>
        /// Confirm Exit Method called from the Button
        /// </summary>
        protected virtual void ConfirmExit()
        {
            // To escape a dirty component set IsClean manually and navigate.
            this.Service.SetClean();
            this.BrowserService.SetExitCheck(false);
            // Check if we have a Url the user tried to navigate to - default exit to the root
            if (!string.IsNullOrEmpty(this.RouterSessionService.NavigationCancelledUrl)) this.NavManager.NavigateTo(this.RouterSessionService.NavigationCancelledUrl);
            else if (!string.IsNullOrEmpty(this.ReturnPageUrl)) this.NavManager.NavigateTo(this.ReturnPageUrl);
            else this.NavManager.NavigateTo("/");
        }

        public override void Dispose()
        {
            this.RouterSessionService.NavigationCancelled -= this.OnNavigationCancelled;
            base.Dispose();
        }

    }
}
