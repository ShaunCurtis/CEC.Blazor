using System;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Interface for the Paging Component
    /// </summary>
    public interface IDbRecord<T>
    {
        public int ID { get; }

        public string DisplayName { get; }

        /// <summary>
        /// Creates a deep copy of the object
        /// </summary>
        /// <returns></returns>
        public T ShadowCopy(); 

    }
}
