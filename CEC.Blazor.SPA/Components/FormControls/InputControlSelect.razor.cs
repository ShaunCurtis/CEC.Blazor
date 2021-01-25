using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using CEC.FormControls.Components.FormControls;

namespace CEC.Blazor.SPA.Components.FormControls
{
    public partial class InputControlSelect<T> : FormRecordControlBase<T>
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Label { get; set; }

        [Parameter]
        public Expression<Func<T>> ValidationFor { get; set; }

        [Parameter]
        public bool ShowDefaultOption { get; set; } = true;

        [Parameter]
        public bool DefaultOptionSelectable { get; set; } = false;

        [Parameter]
        public T DefaultKey { get; set; } = default;

        [Parameter]
        public string DefaultValue { get; set; } = "--- Select a Value ---";

        [Parameter]
        public string LockedDisplayValue { get; set; }

        [Parameter]
        public SortedDictionary<T, string> OptionList { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected bool IsSelected(object value) => value.Equals(this.Value);

        protected bool isOptionList { get => OptionList != null; }

        protected void ValueHasChanged(ChangeEventArgs e)
        {
            this.CurrentValueAsString = e.Value.ToString();
        }

        protected override string FormatValueAsString(T value)
        {
            return base.FormatValueAsString(value);
        }

        protected override bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
        {
            var tripped = false;

            validationErrorMessage = null;
            result = default;
            if (typeof(T) == typeof(string)) result = (T)(object)value;
            else if (typeof(T) == typeof(int))
            {
                int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue);
                result = (T)(object)parsedValue;
            }
            else if (typeof(T) == typeof(long))
            {
                long.TryParse(value, out var parsedValue);
                result = (T)(object)parsedValue;
            }
            else if (typeof(T) == typeof(Guid))
            {
                Guid.TryParse(value, out var parsedValue);
                result = (T)(object)parsedValue;
            }
            else if (typeof(T).IsEnum)
            {
                try { result = (T)Enum.Parse(typeof(T), value); }
                catch (ArgumentException)
                {
                    result = default;
                    validationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
                    tripped = true;
                }
            }
            if (tripped) throw new InvalidOperationException($"{GetType()} does not support the type '{typeof(T)}'.");
            return !tripped;
        }
    }
}
