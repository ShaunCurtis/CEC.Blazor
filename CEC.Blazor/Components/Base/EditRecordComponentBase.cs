using CEC.Blazor.Components.UIControls;
using CEC.Blazor.Data;
using CEC.Routing.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.Base
{
    public class EditRecordComponentBase<TRecord, TContext> : 
        RecordComponentBase<TRecord, TContext>, 
        IRecordRoutingComponent 
        where TRecord : class, IDbRecord<TRecord>, new()
       where TContext : DbContext
    {
        /// <summary>
        /// This Page URL/route
        /// </summary>
        [Obsolete]
        public string PageUrl { get; set; }

        /// <summary>
        /// This Route URL
        /// </summary>
        public string RouteUrl { get; set; }

        /// <summary>
        /// RouterDelay
        /// </summary>
        public virtual int RouterDelay => 100;

        /// <summary>
        /// Boolean Property controlling Routing
        /// </summary>
        public bool IsClean => this.Service?.IsClean ?? true;

        /// <summary>
        /// Boolean Property set when a navigation event has been cancelled
        /// </summary>
        public bool NavigationCancelled { get; set; }

        /// <summary>
        /// Boolean Property used to control display of confirm exit button
        /// </summary>
        public bool ShowExitConfirmation { get; set; }

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
        protected async override Task LoadRecordAsync()
        {
            await base.LoadRecordAsync();

            //set up the Edit Context
            this.EditContext = new EditContext(this.Service.Record);

            // Get the actual page Url from the Navigation Manager
            this.RouteUrl = this.NavManager.Uri;
            // Set up the router service
            this.RouterSessionService.ActiveComponent = this;
            this.RouterSessionService.NavigationCancelled += this.OnNavigationCancelled;
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                this.RouterSessionService.SameComponentNavigation += this.OnSameRouteRouting;
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
            this.ShowExitConfirmation = true;
            this.AlertMessage.SetAlert("<b>THIS RECORD ISN'T SAVED</b>. Either <i>Save</i> or <i>Exit Without Saving</i>.", Bootstrap.ColourCode.danger);
            InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Event handler for the RecordFromControls FieldChanged Event
        /// </summary>
        /// <param name="isdirty"></param>
        protected virtual void RecordFieldChanged(bool isdirty)
        {
            if (this.EditContext != null)
            {
                this.Service.SetClean(!isdirty);
                this.ShowExitConfirmation = false;
                this.NavigationCancelled = false;
                if (this.IsClean)
                {
                    this.AlertMessage.ClearAlert();
                    this.RouterSessionService.SetPageExitCheck(false);
                }
                else
                {
                    this.AlertMessage.SetAlert("The Record isn't Saved", Bootstrap.ColourCode.warning);
                    this.RouterSessionService.SetPageExitCheck(true);
                }
                this.UpdateUI();
                InvokeAsync(this.StateHasChanged);
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
            this.ShowExitConfirmation = false;
            this.NavigationCancelled = false;
            this.RouterSessionService.NavigationCancelledUrl = string.Empty;
            if (this.IsClean) this.AlertMessage.ClearAlert();
            else this.AlertMessage.SetAlert($"{this.Service.RecordConfiguration.RecordDescription} Changed", Bootstrap.ColourCode.warning);
            this.UpdateState();
        }

        /// <summary>
        /// Save Method called from the Button
        /// </summary>
        protected virtual async Task<bool> Save()
        {
            var ok = false;
            if (this.EditContext.Validate())
            {
                ok = await this.Service.SaveRecordAsync();
                if (ok)
                {
                    // Set the EditContext State
                    this.EditContext.MarkAsUnmodified();
                    this.ShowExitConfirmation = false;
                    this.RouterSessionService.NavigationCancelledUrl = string.Empty;
                }
                // Set the alert message to the return result
                this.AlertMessage.SetAlert(this.Service.TaskResult);
                this.UpdateState();
            }
            else this.AlertMessage.SetAlert("A validation error occurred.  Check individual fields for the relevant error.", Bootstrap.ColourCode.danger);
            return ok;
        }

        /// <summary>
        /// Save and Exit Method called from the Button
        /// </summary>
        protected virtual async void SaveAndExit()
        {
            if (await this.Save()) this.ConfirmExit();
        }

        /// <summary>
        /// Confirm Exit Method called from the Button
        /// </summary>
        protected virtual void Exit()
        {
            if (this.IsClean) ConfirmExit();
            else this.ShowExitConfirmation = true;
        }

        /// <summary>
        /// Confirm Exit Method called from the Button
        /// </summary>
        protected virtual void ConfirmExit()
        {
            // To escape a dirty component set IsClean manually and navigate.
            this.Service.SetClean();
            this.RouterSessionService.NavigationCancelledUrl = string.Empty;
            //turn off page exit checking
            this.RouterSessionService.SetPageExitCheck(false);
            if (this.IsModal) ModalExit();
            else
            {
                // Check if we have a Url the user tried to navigate to - default exit to the root
                if (!string.IsNullOrEmpty(this.RouterSessionService.NavigationCancelledUrl)) this.NavManager.NavigateTo(this.RouterSessionService.NavigationCancelledUrl);
                else if (!string.IsNullOrEmpty(this.RouterSessionService.ReturnRouteUrl)) this.NavManager.NavigateTo(this.RouterSessionService.ReturnRouteUrl);
                else this.NavManager.NavigateTo("/");
            }
        }

        public override void Dispose()
        {
            this.RouterSessionService.NavigationCancelled -= this.OnNavigationCancelled;
            base.Dispose();
        }

    }
}
