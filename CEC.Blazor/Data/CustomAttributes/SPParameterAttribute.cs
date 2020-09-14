using System;
using System.Data;
using System.Reflection;

namespace CEC.Blazor.Data
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SPParameterAttribute : Attribute
    {
        /// <summary>
        /// Name used in the Stored Procedure
        /// </summary>
        public string ParameterName = string.Empty;

        /// <summary>
        /// SQL Data Type
        /// </summary>
        public SqlDbType DataType = SqlDbType.VarChar;

        /// <summary>
        /// Defines if this is the ID field for the Record
        /// Only set one per Record
        /// </summary>
        public bool IsID = false;

        /// <summary>
        /// Defines if this is the Description field for a lookup list
        /// Only set one per Record
        /// </summary>
        public bool IsLookUpDecription = false;

        public SPParameterAttribute() { }

        public SPParameterAttribute(string pName, SqlDbType dataType, bool isID = false)
        {
            this.ParameterName = pName;
            this.DataType = dataType;
            this.IsID = isID;
        }

        /// <summary>
        /// Method to check if we have a name and if not set it to the property name
        /// </summary>
        /// <param name="prop"></param>
        public void CheckName( PropertyInfo prop) => this.ParameterName = string.IsNullOrEmpty(this.ParameterName) ? $"@{prop.Name}" : this.ParameterName;

    }
}
