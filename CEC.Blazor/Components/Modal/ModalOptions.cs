using System.Collections.Generic;

namespace CEC.Blazor.Components.Modal
{
    public class ModalOptions
    {
        public string Title { get; set; } = "Modal Dialog";

        public bool ShowCloseButton { get; set; }

        public bool HideHeader { get; set; }

        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public bool GetParameter(string key, out object value)
        {
            value = null;
            if (this.Parameters.ContainsKey(key)) value = this.Parameters[key];
            return this.Parameters.ContainsKey(key);
        }

        public object GetParameter(string key)
        {
            if (this.Parameters.ContainsKey(key)) return this.Parameters[key];
            else return null;
        }

        public string GetParameterAsString(string key)
        {
            if (this.Parameters.ContainsKey(key) && this.Parameters[key] is string) return (string)this.Parameters[key];
            else return string.Empty;
        }

        public void SetParameter(string key, object value)
        {
            if (this.Parameters.ContainsKey(key)) this.Parameters[key] = value;
            else this.Parameters.Add(key, value); 
        }

    }
}
