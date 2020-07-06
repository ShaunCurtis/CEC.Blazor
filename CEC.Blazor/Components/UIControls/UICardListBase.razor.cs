using Microsoft.AspNetCore.Components;
using CEC.Blazor.Components.Base;
using CEC.Blazor.Data;
using CEC.Blazor.Components;
using System;
using CEC.Routing.Services;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardListBase : UIBase, IRecordNavigation
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


        [CascadingParameter]
        public RecordConfigurationData RecordConfiguration { get; set; }

        [CascadingParameter(Name = "ID")]
        public int CascadeID { get; set; } = 0;

        [CascadingParameter(Name = "OnView")]
        protected Action<int> OnView { get; set; }

        [CascadingParameter(Name = "OnEdit")]
        protected Action<int> OnEdit { get; set; }

        [Parameter]
        public bool MaxRow { get; set; }

        [Parameter]
        public bool Show { get; set; } = true;

        [Parameter]
        public bool IsNavigation { get; set; } = true;

        [Parameter]
        public int ID { get; set; } = 0;

        private int _ID { get => this.CascadeID > 0 ? this.CascadeID : this.ID; }

        private bool DoNavigation => this._ID > 0 && IsNavigation;

        protected virtual string _RowCss => "";

        private string RowCss => string.IsNullOrEmpty(this.AddOnCss) ? _RowCss : string.Concat(_RowCss, " ", this.AddOnCss);

        protected void NavigateToView()
        {
            if (this.OnView is null) ((IRecordNavigation)this).NavigateTo(new EditorEventArgs(PageExitType.ExitToView, this.ID, this.RecordConfiguration.RecordName));
            else this.OnView.Invoke(this._ID);
        }

        protected void NavigateToEditor()
        {
            if (this.OnView is null) ((IRecordNavigation)this).NavigateTo(new EditorEventArgs(PageExitType.ExitToEditor, this.ID, this.RecordConfiguration.RecordName));
            else this.OnEdit.Invoke(this._ID);
        }
    }
}
