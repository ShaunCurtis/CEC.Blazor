using System;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Interface for the Paging Component
    /// </summary>
    public interface IDbRecord<T>
    {

        /// <summary>
        /// Creates a deep copy of the object
        /// </summary>
        /// <returns></returns>
        public T ShadowCopy(); 

    }
}
