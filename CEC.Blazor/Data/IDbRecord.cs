using System;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Interface for the Paging Component
    /// </summary>
    public interface IDbRecord<T>
    {
        /// <summary>
        /// ID to ensure we have a unique key
        /// </summary>
        public int ID { get; }

        public string DisplayName { get; }

        /// <summary>
        /// Creates a deep copy of the object
        /// </summary>
        /// <returns></returns>
        public T ShadowCopy(); 

    }
}
