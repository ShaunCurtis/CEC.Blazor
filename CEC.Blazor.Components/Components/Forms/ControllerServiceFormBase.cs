﻿using System;
using Microsoft.AspNetCore.Components;
using CEC.Blazor.Services;
using CEC.Blazor.Data;
using CEC.Blazor.Components.UIControls;
using Microsoft.EntityFrameworkCore;
using CEC.Blazor.Components;

namespace CEC.Blazor.Components.Forms
{
    public class ControllerServiceFormBase<TRecord, TContext> : 
        FormBase 
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext
    {

        /// <summary>
        /// Service with IDataRecordService Interface that corresponds to Type T
        /// Normally set as the first line in the Page OnInitialized event.
        /// </summary>
        public IControllerService<TRecord, TContext> Service { get; set; }

        /// <summary>
        /// Property to control various UI Settings
        /// Used as a cascadingparameter
        /// </summary>
        [Parameter] public UIOptions UIOptions { get; set; } = new UIOptions();

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

    }
}
