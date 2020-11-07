using System;
using Microsoft.AspNetCore.Components;
using CEC.Blazor.Services;
using CEC.Blazor.Data;
using CEC.Blazor.Components.UIControls;
using Microsoft.EntityFrameworkCore;
using CEC.Blazor.Components.Base;

namespace CEC.Blazor.Components.BaseForms
{
    public class ControllerServiceFormBase<T, TContext> : 
        FormBase 
        where T : class, IDbRecord<T>, new()
        where TContext : DbContext
    {

        /// <summary>
        /// Service with IDataRecordService Interface that corresponds to Type T
        /// Normally set as the first line in the Page OnInitialized event.
        /// </summary>
        public IControllerService<T, TContext> Service { get; set; }

        /// <summary>
        /// Property to control various UI Settings
        /// Used as a cascadingparameter
        /// </summary>
        [Parameter] public UIOptions UIOptions { get; set; } = new UIOptions();

        /// <summary>
        /// An additional property that can be set in routing for cutom load actions
        /// Check individual record routing for specifics 
        /// </summary>
        [Parameter] public int? Action { get; set; } = 0;

        /// <summary>
        /// The default alert used by all inherited components
        /// Used for Editor Alerts, error messages, ....
        /// </summary>
        [Parameter] public Alert AlertMessage { get; set; } = new Alert();

        /// <summary>
        /// Property with generic error message for the Page Manager 
        /// </summary>
        protected virtual string RecordErrorMessage { get; set; } = "The Application is loading the record.";

        /// <summary>
        /// Boolean check if the Service exists
        /// </summary>
        protected bool IsService { get => this.Service != null; }

        /// <summary>
        /// Event Method for triggering a UI update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UIStateChanged(object sender, EventArgs e) => InvokeAsync(this.Render);

        /// <summary>
        /// Sets ther alert with a Danger Message
        /// </summary>
        protected void ClearAlert()
        {
            this.AlertMessage.ColourCode = Bootstrap.ColourCode.info;
            this.AlertMessage.IsAlert = false;
            this.AlertMessage.Message = "All Clear";
        }

        /*===================================================================
         *  Section contains the Navigation methods used by the List/Viewer/Editor pages.
         *  All use the Record Configuration obect obtained from the DataService Service to help 
         *  define the actual  routing path
         ===================================================================-*/

        /// <summary>
        /// Generic Navigator
        /// </summary>
        /// <param name="id"></param>
        protected virtual void NavigateTo(PageExitType exittype)
        {
            // Calls the main NavigateTo Method
            this.NavigateTo(new EditorEventArgs(exittype));
        }

        /// <summary>
        /// Generic Navigator
        /// </summary>
        /// <param name="id"></param>
        protected override void NavigateTo(EditorEventArgs e)
        {
            if (IsService)
            {
                //check if record name is populated and if not populates it
                if (string.IsNullOrEmpty(e.RecordName)) e.RecordName = this.Service.RecordConfiguration.RecordName;
            }
        }
    }
}
