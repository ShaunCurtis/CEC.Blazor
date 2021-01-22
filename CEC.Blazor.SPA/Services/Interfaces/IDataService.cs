using CEC.Blazor.Components;
using CEC.Blazor.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CEC.Blazor.Services
{
    public interface IDataService<TRecord, TContext> 
        where TRecord : class, new() 
        where TContext : DbContext
    {
        public HttpClient HttpClient { get; set; }

        public IDbContextFactory<TContext> DBContext { get; set; }

        public IConfiguration AppConfiguration { get; set; }

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
        /// Method to get the current record count
        /// </summary>
        /// <returns></returns>
        public Task<int> GetRecordListCountAsync() => Task.FromResult(0);

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
        public Task<DbTaskResult> CreateRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// Method to delete a record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DbTaskResult> DeleteRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// Method to get a dictionary of id/name for a record type
        /// Used in Lookup Lists
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public Task<SortedDictionary<int, string>> GetLookupListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup> => Task.FromResult(new SortedDictionary<int,string>());

        /// <summary>
        /// Method to get a dictionary of distinct values for a field in a record type
        /// Used in Lookup Lists
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public Task<List<string>> GetDistinctListAsync(DbDistinctRequest req) => Task.FromResult(new List<string>());
        
        /// <summary>
        /// Method to get a base record set from an IDbRecord record
        /// </summary>
        /// <returns></returns>
        public Task<List<DbBaseRecord>> GetBaseRecordListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup> => Task.FromResult(new List<DbBaseRecord>());

        /// <summary>
        /// Method to build the a list of SqlParameters for a CUD Stored Procedure
        /// </summary>
        /// <param name="item"></param>
        /// <param name="withid"></param>
        /// <returns></returns>
        public List<SqlParameter> GetSQLParameters(TRecord item, bool withid = false) => new List<SqlParameter>();

    }
}
