using CEC.Blazor.Components;
using CEC.Blazor.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Extensions
{
    public static class DbContextExtensions
    {

        /// <summary>
        /// Extension Method to run a parameterized Store Procedure using an Entity Framework Context DBConnection
        /// </summary>
        /// <param name="context"></param>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<bool> ExecStoredProcAsync(this DbContext context, string storedProcName, List<SqlParameter> parameters)
        {
            var result = false;

            var cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = storedProcName;
            cmd.CommandType = CommandType.StoredProcedure;
            parameters.ForEach(item => cmd.Parameters.Add(item));
            using (cmd)
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch {}
                finally
                {
                    cmd.Connection.Close();
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Generic Method to get a record List from a DbSet
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="context"></param>
        /// <param name="dbSetName"></param>
        /// <returns></returns>
        public async static Task<List<TRecord>> GetRecordListAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
        {
            var dbset = GetDbSet<TRecord>(context, dbSetName);
            return await dbset.ToListAsync();
        }

        /// <summary>
        /// Generic Method to get a filtered record List from a DbSet
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="context"></param>
        /// <param name="filterList"></param>
        /// <param name="dbSetName"></param>
        /// <returns></returns>
        public async static Task<List<TRecord>> GetRecordFilteredListAsync<TRecord>(this DbContext context, IFilterList filterList, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
        {
            var par = context.GetType().GetProperty(dbSetName ?? IDbRecord<TRecord>.RecordName);
            var set = par.GetValue(context);
            var dbset = (DbSet<TRecord>)set;
            var list = new List<TRecord>();
            // if we have a filter go through each filter
            if (filterList != null && filterList.Filters.Count > 0)
            {
                foreach (var filter in filterList.Filters)
                {
                    // Get the filter propertyinfo object
                    var x = typeof(TRecord).GetProperty(filter.Key);
                    // if we have a list already apply the filter to the list
                    if (list.Count > 0) list.Where(item => x.GetValue(item) == filter.Value).ToList();
                    // If we have an empty list we can query the database directly
                    else list = await dbset.FromSqlRaw($"SELECT * FROM vw_{ par.Name} WHERE {filter.Key} = {filter.Value}").ToListAsync();
                }
            }
            //  No list, just get the full list
            else list = await dbset.ToListAsync();
            return list;
        }

        /// <summary>
        /// Generic Method to get a record List count from a DbSet
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="context"></param>
        /// <param name="dbSetName"></param>
        /// <returns></returns>
        public async static Task<int> GetRecordListCountAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
        {
            var dbset = GetDbSet<TRecord>(context, dbSetName);
            return await dbset.CountAsync();
        }

        /// <summary>
        /// Generic Method to get a record from a DbSet
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <param name="dbSetName"></param>
        /// <returns></returns>
        public async static Task<TRecord> GetRecordAsync<TRecord>(this DbContext context, int id, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
        {
            var dbset = GetDbSet<TRecord>(context, dbSetName);
            return await dbset.FirstOrDefaultAsync(item => ((IDbRecord<TRecord>)item).ID == id);
        }

        /// <summary>
        /// Generic Method to get a lookuplist from a DbSet
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="context"></param>
        /// <param name="dbSetName"></param>
        /// <returns></returns>
        public async static Task<SortedDictionary<int, string>> GetRecordLookupListAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
        {

            var list = new SortedDictionary<int, string>();
            var dbset = GetDbSet<TRecord>(context, dbSetName);
            await dbset.ForEachAsync(item => list.Add(item.ID, item.DisplayName));
            return list;
        }

        /// <summary>
        /// Method to get the DBSet from TRecord or a Name
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="context"></param>
        /// <param name="dbSetName"></param>
        /// <returns></returns>
        private static DbSet<TRecord> GetDbSet<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
        {
            // Get the property info object for the DbSet 
            var pinfo = context.GetType().GetProperty(dbSetName ?? IDbRecord<TRecord>.RecordName);
            // Get the property DbSet
            var set = pinfo.GetValue(context);
            return (DbSet<TRecord>)set;
        }

    }
}
