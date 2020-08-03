using Microsoft.AspNetCore.Components;
using CEC.Blazor.Data;
using System;
using CEC.Routing.Services;

namespace CEC.Blazor.Components.UIControls
{
    public class UICardListBase : UIComponent, IRecordNavigation
    {
        /// <summary>
        /// Injected Router Session Object
        /// </summary>
        [Inject]
        public RouterSessionService RouterSessionService { get; set; }

        /// <summary>
        /// Injected Navigation Service
        /// </summary>
        [Inject]
        public NavigationManager NavManager { get; set; }

        /// <summary>
        /// Record Configuration
        /// </summary>
        [CascadingParameter]
        public RecordConfigurationData RecordConfiguration { get; set; }

        /// <summary>
        /// Record ID passed via a cascade
        /// </summary>
        [CascadingParameter(Name = "RecordID")]
        public int RecordID { get; set; } = 0;

        /// <summary>
        /// On View Event
        /// </summary>
        [CascadingParameter(Name = "OnView")]
        protected Action<int> OnView { get; set; }

        /// <summary>
        /// OnEdit Event
        /// </summary>
        [CascadingParameter(Name = "OnEdit")]
        protected Action<int> OnEdit { get; set; }

        /// <summary>
        /// Boolena to define if each line should navigate to the record Viewer
        /// </summary>
        [Parameter]
        public bool IsNavigation { get; set; } = true;

        /// <summary>
        /// Record ID passed directly to the control
        /// </summary>
        [Parameter]
        public int ID { get; set; } = 0;

        /// <summary>
        /// Record ID used by the control
        /// </summary>
        protected int _ID { get => this.RecordID > 0 ? this.RecordID : this.ID; }

        /// <summary>
        /// Boolean for UI to check if navigation is ok 
        /// </summary>
        protected bool DoNavigation => this._ID > 0 && IsNavigation;

        /// <summary>
        /// inherited
        /// </summary>
        protected virtual string _RowCss => "";

        /// <summary>
        /// Css for the Row
        /// </summary>
        protected string RowCss => string.IsNullOrEmpty(this.AddOnCss) ? _RowCss : string.Concat(_RowCss, " ", this.AddOnCss);

        /// <summary>
        /// View Navigation hanlder
        /// </summary>
        public void NavigateToView(int id = -1)
        {
            id = id == -1 ? this._ID : id;
            if (this.OnView is null) ((IRecordNavigation)this).NavigateTo(new EditorEventArgs(PageExitType.ExitToView, id, this.RecordConfiguration.RecordName));
            else this.OnView.Invoke(id);
        }

        /// <summary>
        /// Editor navigation handler
        /// </summary>
        public void NavigateToEditor(int id = -1)
        {
            id = id == -1 ? this._ID : id;
            if (this.OnView is null) ((IRecordNavigation)this).NavigateTo(new EditorEventArgs(PageExitType.ExitToEditor, id, this.RecordConfiguration.RecordName));
            else this.OnEdit.Invoke(id);
        }
    }
}
