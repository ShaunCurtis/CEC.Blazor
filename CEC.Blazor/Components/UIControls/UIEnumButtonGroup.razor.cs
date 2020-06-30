using CEC.Blazor.Components.Base;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using CEC.Blazor.Extensions;


namespace CEC.Blazor.Components.UIControls
{
    public partial class UIEnumButtonGroup<TEnum> : ApplicationComponentBase where TEnum: Enum
    {
        [Parameter]
        public TEnum Enumerator { get; set; }

        [Parameter]
        public int Value { get; set; }

        [Parameter]
        public string SelectedButtonCss { get; set; } = "btn btn-info";

        [Parameter]
        public string ButtonCss { get; set; } = "btn btn-outline-secondary";

        [Parameter]
        public bool ReadOnly { get; set; }

        private Dictionary<int, string> Values { get; set; } = new Dictionary<int, string>();

        [Parameter]
        public EventCallback<int> ValueChanged { get; set; }


        protected override void OnInitialized()
        {
            this.GetValues();
        }

        protected void GetValues()
        {
            Values.Clear();
            var values = Enum.GetValues(typeof(TEnum));
            foreach (int v in values) this.Values.Add(v, Enum.GetName(typeof(TEnum), v).AsSeparatedString());
        }

        protected async void SetState(int state)
        {
            if (!this.ReadOnly)
            {
                this.Value = state;
                await ValueChanged.InvokeAsync(state);
                this.StateHasChanged();
            }
        }

        protected string GetButtonCss(int value) => value == this.Value ? this.SelectedButtonCss : ButtonCss;
    }
}
