using CEC.Blazor.Components.UIControls;
using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.BaseForms
{
    public class EditRecordFormBase<TRecord, TContext> :
        RecordFormBase<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
       where TContext : DbContext
    {
        /// <summary>
        /// Boolean Property exposing the Service Clean state
        /// </summary>
        public bool IsClean => this.Service?.IsClean ?? true;

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
        /// property used by the UIErrorHandler component
        /// </summary>
        protected override bool IsError { get => !(this.IsRecord && this.EditContext != null); }

        /// <summary>
        /// Inherited - Always call the base method first
        /// </summary>
        protected async override Task LoadRecordAsync(bool firstLoad = false)
        {
            await base.LoadRecordAsync(firstLoad);
            //set up the Edit Context
            this.EditContext = new EditContext(this.Service.Record);
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                this.Service.OnDirty += this.OnRecordDirty;
                this.Service.OnClean += this.OnRecordClean;
            }
        }

        protected void OnRecordDirty(object sender, EventArgs e)
        {
            this.ViewManager.IsLocked = true;
            this.ViewManager.SetPageExitCheck(true);
            this.AlertMessage.SetAlert("The Record isn't Saved", Bootstrap.ColourCode.warning);
            InvokeAsync(this.Render);
        }

        protected void OnRecordClean(object sender, EventArgs e)
        {
            this.ViewManager.IsLocked = false;
            this.ViewManager.SetPageExitCheck(false);
            this.AlertMessage.ClearAlert();
            InvokeAsync(this.Render);
        }

        /// <summary>
        /// Event handler for the RecordFromControls FieldChanged Event
        /// </summary>
        /// <param name="isdirty"></param>
        protected virtual void RecordFieldChanged(bool isdirty)
        {
            if (this.EditContext != null) this.Service.SetDirtyState(isdirty);
        }

        /// <summary>
        /// Save Method called from the Button
        /// </summary>
        protected virtual async Task<bool> Save()
        {
            var ok = false;
            // Validate the EditContext
            if (this.EditContext.Validate())
            {
                // Save the Record
                ok = await this.Service.SaveRecordAsync();
                if (ok)
                {
                    // Set the EditContext State
                    this.EditContext.MarkAsUnmodified();
                }
                // Set the alert message to the return result
                this.AlertMessage.SetAlert(this.Service.TaskResult);
                // Trigger a component State update - buttons and alert need to be sorted
                await RenderAsync();
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
        protected virtual void TryExit()
        {
            // Check if we are free to exit ot need confirmation
            if (this.IsClean) ConfirmExit();
        }

        /// <summary>
        /// Confirm Exit Method called from the Button
        /// </summary>
        protected virtual void ConfirmExit()
        {
            // To escape a dirty component set IsClean manually and navigate.
            this.Service.SetDirtyState(false);
            // Sort the exit strategy
            this.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            this.Service.OnDirty -= this.OnRecordDirty;
            this.Service.OnClean -= this.OnRecordClean;
            base.Dispose(disposing);
        }
    }
}
