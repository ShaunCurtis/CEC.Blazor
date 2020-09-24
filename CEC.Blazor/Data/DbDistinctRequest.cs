using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Class to holder Database Access Information for a IDbRecord
    /// </summary>
    public class DbDistinctRequest
    {
        public string FieldName { get; set; }

         public string DistinctSetName { get; set; }

        public string QuerySetName { get; set; }
    }
}
