using CEC.Blazor.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Utilities;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace CEC.Blazor.Services
{
    public interface IDataService<TRecord> where TRecord : new()
    {

        /// <summary>
        /// Record Configuration Property
        /// </summary>
        public RecordConfigurationData RecordConfiguration { get; set; }

        /// <summary>
        /// Method to get the Record List
        /// </summary>
        /// <returns></returns>
        public Task<List<TRecord>> GetRecordListAsync() => Task.FromResult(new List<TRecord>());

        /// <summary>
        /// Method to get a filtered Record List
        /// </summary>
        /// <returns></returns>
        public Task<List<TRecord>> GetFilteredRecordListAsync(IFilterList filterList) => Task.FromResult(new List<TRecord>());

        /// <summary>
        /// Method to get a Record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<TRecord> GetRecordAsync(int id) => Task.FromResult(new TRecord());

        /// <summary>
        /// Method to update a record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public Task<DbTaskResult> UpdateRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// method to add a record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public Task<DbTaskResult> AddRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// Method to delete a record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DbTaskResult> DeleteRecordAsync(int id) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// Method to build the a list of SqlParameters for a CUD Stored Procedure
        /// </summary>
        /// <param name="item"></param>
        /// <param name="withid"></param>
        /// <returns></returns>
        public List<SqlParameter> GetSQLParameters(TRecord item, bool withid = false) => new List<SqlParameter>();

    }
}
