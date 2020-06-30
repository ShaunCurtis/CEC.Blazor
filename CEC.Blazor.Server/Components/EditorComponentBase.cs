using CEC.Blazor.Data;
using CEC.Routing.Components;
using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Components
{
    public class EditorComponentBase : ComponentBase, IRecordRoutingComponent
    {

        /// <summary>
        /// Injected Navigation Manager
        /// </summary>
        [Inject]
        public NavigationManager NavManager { get; set; }

        /// <summary>
        /// Injected User Session Object
        /// </summary>
        [Inject]
        public RouterSessionService RouterSessionService { get; set; }

        /// <summary>
        /// IRecordRoutingComponent implementation
        /// </summary>
        public string PageUrl { get; set; }

        /// <summary>
        /// IRecordRoutingComponent implementation
        /// </summary>
        public bool IsClean { get; set; } = true;

        /// <summary>
        /// Boolean property set when the user attempts to exit a dirty component
        /// </summary>
        protected bool ExitAttempt { get; set; }

        /// <summary>
        /// Form Edit Context
        /// </summary>
        public EditContext EditContext { get; set; }

        /// <summary>
        /// Alert object used in UI by UI Alert
        /// </summary>
        public Alert Alert { get; set; } = new Alert();

        protected override Task OnInitializedAsync()
        {
            this.PageUrl = this.NavManager.Uri;
            this.RouterSessionService.ActiveComponent = this;
            this.RouterSessionService.NavigationCancelled += OnNavigationCancelled;
            return base.OnInitializedAsync();
        }

        /// <summary>
        /// Event Handler for the Navigation Cancelled event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnNavigationCancelled(object sender, EventArgs e)
        {
            this.ExitAttempt = true;
            this.Alert.SetAlert("<b>RECORD ISN'T SAVED</b>. Either Cancel or Exit Without Saving.", Alert.AlertDanger);
            this.StateHasChanged();
        }

        /// <summary>
        /// Event handler for the RecordFromControls FieldChanged Event
        /// </summary>
        /// <param name="changestate"></param>
        protected virtual void RecordFieldChanged(bool changeState)
        {
            if (this.EditContext != null)
            {
                this.ExitAttempt = false;
                this.IsClean = !changeState;
                    if (this.IsClean) this.Alert.ClearAlert();
                else this.Alert.SetAlert("The Weather has changed!", Alert.AlertWarning);
            }
        }


    }
}
