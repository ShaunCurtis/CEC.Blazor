using CEC.Blazor.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Blazor.Services
{
    public interface IDataService<T> where T : new()
    {
        /// <summary>
        /// Record Configuration Property
        /// </summary>
        public RecordConfigurationData RecordConfiguration { get; set; }

        /// <summary>
        /// Method to get the Record List
        /// </summary>
        /// <returns></returns>
        public Task<List<T>> GetRecordListAsync() => Task.FromResult(new List<T>());

        /// <summary>
        /// Method to get a Record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<T> GetRecordAsync(int id) => Task.FromResult(new T());

        /// <summary>
        /// Method to update a record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public Task<DbTaskResult> UpdateRecordAsync(T record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// method to add a record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public Task<DbTaskResult> AddRecordAsync(T record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// Method to delete a record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DbTaskResult> DeleteRecordAsync(int id) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });
    }
}
