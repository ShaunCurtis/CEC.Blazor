using System;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Interface for the Paging Component
    /// </summary>
    public interface IDbRecord<TRecord>
    {
        /// <summary>
        /// ID to ensure we have a unique key
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Display name for the Record
        /// Point to the field that you want to use for the dipslay name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Property to get the default RecordName from the Class Name
        /// </summary>
        public static string RecordName => typeof(TRecord).Name.Replace("Db", "");

        /// <summary>
        /// Creates a deep copy of the object
        /// </summary>
        /// <returns></returns>
        public TRecord ShadowCopy();

        /// <summary>
        /// Set the record ID to 0 to represent a new record
        /// default value should be -1
        /// </summary>
        public void SetNew();

    }
}
