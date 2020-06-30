using CEC.Blazor.Data;
using CEC.Blazor.Server.Components;
using CEC.Blazor.Server.Data;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Pages
{
    public partial class WeatherForecastEditor : EditorComponentBase
    {
        public WeatherForecast Record { get; set; } = new WeatherForecast();

        public WeatherForecast ShadowRecord { get; set; }

        protected string CardBorderColour => this.IsClean ? "border-secondary" : "border-danger";

        protected string CardHeaderColour => this.IsClean ? "bg-secondary text-white" : "bg-danger text-white";

        protected override Task OnInitializedAsync()
        {
            // Set up the Edit Context
            this.EditContext = new EditContext(this.Record);

            // Make a copy of the existing record - in this case it's always new but in the real world that won't be the case
            this.ShadowRecord = this.Record.ShadowCopy();

            // Get the actual page Url from the Navigation Manager
            this.PageUrl = this.NavManager.Uri;

            return base.OnInitializedAsync();
        }

        /// <summary>
        /// Save Method called from the Button
        /// </summary>
        protected void Save()
        {
            // Set the Shadow Copy to a copy of the current record
            // Normally run a Save/Create CRUD operation here
            this.ShadowRecord = this.Record.ShadowCopy();
            // Set the EditContext State
            this.EditContext.MarkAsUnmodified();
            this.IsClean = true;
            this.Alert.SetAlert("Forecast Saved", Alert.AlertSuccess);
            this.StateHasChanged();
        }

        /// <summary>
        /// Cancel Method called from the Button
        /// </summary>
        protected void Cancel()
        {
            this.ExitAttempt = false;
            if (this.IsClean) this.Alert.ClearAlert();
            else this.Alert.SetAlert("Forecast Changed", Alert.AlertWarning);
            this.StateHasChanged();
        }

        /// <summary>
        /// Confirm Exit Method called from the Button
        /// </summary>
        protected void ConfirmExit()
        {
            // To escape a dirty component set IsClean manually and navigate.
            this.IsClean = true;
            if (!string.IsNullOrEmpty(this.RouterSessionService.NavigationCancelledUrl)) this.NavManager.NavigateTo(this.RouterSessionService.NavigationCancelledUrl);
            else this.NavManager.NavigateTo("/");
        }

    }
}
