using System;

namespace CEC.Blazor.Services
{
    /// <summary>
    /// Interface for the Paging Component
    /// </summary>
    public interface IPagingService
    {
        /// <summary>
        /// Event triggered when the list has changed
        /// </summary>
        event EventHandler ListHasChanged;

    }
}
