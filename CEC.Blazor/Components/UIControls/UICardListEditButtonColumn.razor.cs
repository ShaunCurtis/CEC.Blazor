using Microsoft.AspNetCore.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Components.Base;
using CEC.Blazor.Components;
using System;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardListEditButtonColumn : ApplicationComponentBase
    {

        [CascadingParameter]
        public UIOptions UIOptions { get; set; }

        [CascadingParameter]
        public RecordConfigurationData RecordConfiguration { get; set; }

        [CascadingParameter(Name = "ID")]
        public int CascadeID { get; set; } = 0;

        [CascadingParameter(Name = "OnEdit")]
        protected Action<int> OnEdit { get; set; }

        [Parameter]
        public int ID { get; set; } = 0;

        private int _ID { get => this.CascadeID > 0 ? this.CascadeID : this.ID ; }

        public bool Show => UIOptions?.ShowEdit ?? true;

        [Parameter]
        public bool IsHeader { get; set; }

        private void Navigate() {
            if (this.OnEdit is null) this.NavigateTo(new EditorEventArgs(PageExitType.ExitToView, this._ID, this.RecordConfiguration.RecordName));
            else this.OnEdit.Invoke(this._ID);
        }

    }
}
