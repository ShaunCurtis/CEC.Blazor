using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Class to holder Database Access Information for a IDbRecord
    /// </summary>
    public class DbRecordInfo
    {
        /// <summary>
        /// Create Stored Procedure
        /// </summary>
        public string CreateSP { get; set; }

        /// <summary>
        /// Update Stored Procedure
        /// </summary>
        public string UpdateSP { get; set; }

        /// <summary>
        /// Delete Stored Procedure
        /// </summary>
        public string DeleteSP { get; set; }

        /// <summary>
        /// List of all the Properties with the SPParameter Attribute
        /// </summary>
        public List<PropertyInfo> SPProperties { get; set; } = new List<PropertyInfo>();

    }
}
