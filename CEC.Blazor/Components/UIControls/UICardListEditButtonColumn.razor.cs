using Microsoft.AspNetCore.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Components.Base;
using CEC.Blazor.Components;

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

        [Parameter]
        public int ID { get; set; } = 0;

        private int _ID { get => this.CascadeID > 0 ? this.CascadeID : this.ID ; }

        public bool Show => UIOptions?.ShowEdit ?? true;

        [Parameter]
        public bool IsHeader { get; set; }


        [Parameter]
        public bool IsCustomAction { get; set; }

        [Parameter]
        public EventCallback<long> CustomAction { get; set; }

        private void Navigate() {

            if (this.IsCustomAction) this.CustomAction.InvokeAsync(this._ID);
            else this.NavigateTo(new EditorEventArgs(PageExitType.ExitToEditor, this._ID, this.RecordConfiguration.RecordName));
        }

    }
}
