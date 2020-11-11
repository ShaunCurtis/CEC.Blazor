using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Class to holder Database Access Information for a IDbRecord
    /// </summary>
    public class APIParameters
    {
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Method to get a Parameter value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetParameter(string name, out object value)
        {
            value = null;
            if (this.Parameters.ContainsKey(name)) value = this.Parameters[name];
            return value != null;
        }

        /// <summary>
        /// Method to set a Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetParameter(string name, object value)
        {
            if (Parameters.ContainsKey(name)) this.Parameters[name] = value;
            else Parameters.Add(name, value);
            return Parameters.ContainsKey(name);
        }

        /// <summary>
        /// Method to clear a Parameter out of the Parameters list
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ClearParameter(string name)
        {
            if (Parameters.ContainsKey(name)) this.Parameters.Remove(name);
            return !Parameters.ContainsKey(name);
        }

    }
}
