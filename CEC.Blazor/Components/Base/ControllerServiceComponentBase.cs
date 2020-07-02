using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using CEC.Blazor.Services;
using CEC.Blazor.Data;

namespace CEC.Blazor.Components.Base
{
    public class ControllerServiceComponentBase<T> : ApplicationComponentBase where T : IDbRecord<T>, new()
    {

        /// <summary>
        /// Service with IDataRecordService Interface that corresponds to Type T
        /// Normally set as the first line in the Page OnInitialized event.
        /// </summary>
        public IControlService<T> Service { get; set; }

        /// <summary>
        /// Property for the ID of the record to retrieve.
        /// Normally set by Routing e.g. /Farm/Edit/1
        /// </summary>
        [Parameter] public int? ID { 
            get => this._ID;
            set => this._ID = (value is null) ? 0 : (int)value; 
        }

        /// <summary>
        /// Version of the ID that sets null to 0
        /// </summary>
        public int _ID { get; private set; }

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
        public void UIStateChanged(object sender, EventArgs e) => InvokeAsync(this.StateHasChanged);

        /// <summary>
        /// Sets ther alert with a Danger Message
        /// </summary>
        protected void ClearAlert()
        {
            this.AlertMessage.CSS = Alert.AlertInfo;
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
                //check if record name is populated and if not populate it
                if (string.IsNullOrEmpty(e.RecordName) && e.ExitType == PageExitType.ExitToList) e.RecordName = this.Service.RecordConfiguration.RecordListName;
                else if (string.IsNullOrEmpty(e.RecordName)) e.RecordName = this.Service.RecordConfiguration.RecordName;

                // check if the id is set for view or edit.  If not, set it.
                if (e.ExitType == PageExitType.ExitToEditor || e.ExitType == PageExitType.ExitToView && e.ID == 0) e.ID = this._ID;
                base.NavigateTo(e);
            }
        }
    }
}
