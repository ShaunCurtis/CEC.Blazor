using CEC.Blazor.Components;
using CEC.Blazor.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
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
                catch { }
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
        public async static Task<List<TRecord>> GetRecordListAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class
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
        public async static Task<List<TRecord>> GetRecordFilteredListAsync<TRecord>(this DbContext context, IFilterList filterList, string dbSetName = null) where TRecord : class
        {
            var firstrun = true;
            // Get the PropertInfo object for the record DbSet
            var propertyInfo = context.GetType().GetProperty(dbSetName ?? IDbRecord<TRecord>.RecordName);
            // Get the actual value and cast it correctly
            var dbset = (DbSet<TRecord>)(propertyInfo.GetValue(context));
            // Get a empty list
            var list = new List<TRecord>();
            // if we have a filter go through each filter
            // note that only the first filter runs a SQL query against the database
            // the rest are run against the dataset.  So do the biggest slice with the first filter for maximum efficiency.
            if (filterList != null && filterList.Filters.Count > 0)
            {
                foreach (var filter in filterList.Filters)
                {
                    // Get the filter propertyinfo object
                    var x = typeof(TRecord).GetProperty(filter.Key);
                    // if we have a list already apply the filter to the list
                    if (list.Count > 0) list = list.Where(item => x.GetValue(item).Equals(filter.Value)).ToList();
                    // If this is the first run we query the database directly
                    else if (firstrun) list = await dbset.FromSqlRaw($"SELECT * FROM vw_{ propertyInfo.Name} WHERE {filter.Key} = {filter.Value}").ToListAsync();
                    firstrun = false;
                }
            }
            //  No list, just get the full recordset
            else list = await dbset.ToListAsync();
            return list;
        }

        /// <summary>
        /// Generic Method to get a record List count from a DbSet
        /// You must have a DbSet in your DBContext called dbSetName of type object
        /// public DbSet<object> DistinctList { get; set; }
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="context"></param>
        /// <param name="dbSetName"></param>
        /// <returns></returns>
        public async static Task<List<string>> GetDistinctListAsync(this DbContext context, DbDistinctRequest req)
        {
            var list = new List<string>();
            // wrap in a try as there are many things that can go wrong
            try
            {
                //get the DbDistinct DB Set so we can load the query data into it
                var dbset = GetDbSet<DbDistinct>(context, req.DistinctSetName);
                // Get the data by building the SQL query to run against the view
                var dlist = await dbset.FromSqlRaw($"SELECT DISTINCT(CONVERT(varchar(max), {req.FieldName})) as Value FROM vw_{req.QuerySetName} ORDER BY Value").ToListAsync();
                // Load the results into a string list
                dlist.ForEach(item => list.Add(item.Value));
            }
            catch
            {
                throw new ArgumentException("The SQL Query did not complete.  The most likely cause is one of the DbDistinctRequest parameters is incorrect;");
            }
            return list;
        }

        /// <summary>
        /// Generic Method to get a record List count from a DbSet
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="context"></param>
        /// <param name="dbSetName"></param>
        /// <returns></returns>
        public async static Task<int> GetRecordListCountAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class
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
        public async static Task<TRecord> GetRecordAsync<TRecord>(this DbContext context, int id, string dbSetName = null) where TRecord : class
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
        public async static Task<List<DbBaseRecord>> GetBaseRecordListAsync<TRecord>(this DbContext context) where TRecord : class, IDbRecord<TRecord>
        {
            var list = new List<DbBaseRecord>();
            var dbset = GetDbSet<TRecord>(context, null);
            await dbset.ForEachAsync(item => list.Add(new DbBaseRecord() { ID = item.ID, DisplayName = item.DisplayName }));
            return list;
        }

        /// <summary>
        /// Method to get the DBSet from TRecord or a Name
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="context"></param>
        /// <param name="dbSetName"></param>
        /// <returns></returns>
        private static DbSet<TRecord> GetDbSet<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class
        {
            // Get the property info object for the DbSet 
            var pinfo = context.GetType().GetProperty(dbSetName ?? IDbRecord<TRecord>.RecordName);
            // Get the property DbSet
            return (DbSet<TRecord>)pinfo.GetValue(context);
        }

    }
}
