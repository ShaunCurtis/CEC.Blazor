using Microsoft.AspNetCore.Components;
using CEC.Blazor.Data;
using System;

namespace CEC.Blazor.SPA.Components.UIControls
{
    public class UICardListBase : UIComponent
    {

        /// <summary>
        /// Record ID passed via a cascade
        /// </summary>
        [CascadingParameter(Name = "RecordID")]
        public int RecordID { get; set; } = 0;

        /// <summary>
        /// Boolean to define if each line should navigate to the record Viewer
        /// </summary>
        [Parameter]
        public bool IsNavigation { get; set; } = true;

        /// <summary>
        /// Record ID passed directly to the control
        /// </summary>
        [Parameter]
        public int ID { get; set; } = 0;

        /// <summary>
        /// Record Configuration
        /// </summary>
        public RecordConfigurationData RecordConfiguration => this.UIWrapper?.RecordConfiguration ?? new RecordConfigurationData();

        /// <summary>
        /// Record ID used by the control
        /// </summary>
        protected int _ID => this.RecordID > 0 ? this.RecordID : this.ID;

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
            if (this.UIWrapper.OnView != null) this.UIWrapper.OnView.Invoke(id);
        }

        /// <summary>
        /// Editor navigation handler
        /// </summary>
        public void NavigateToEditor(int id = -1)
        {
            id = id == -1 ? this._ID : id;
            if (this.UIWrapper.OnEdit != null) this.UIWrapper.OnEdit.Invoke(id);
        }
    }
}
