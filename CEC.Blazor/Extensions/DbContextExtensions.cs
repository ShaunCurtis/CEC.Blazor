using CEC.Blazor.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
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

        public async static Task<List<TRecord>> GetRecordListAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
        {
            var par = context.GetType().GetProperty(dbSetName ?? IDbRecord<TRecord>.RecordName);
            var set = par.GetValue(context);
            var sets = (DbSet<TRecord>)set;
            return await sets.ToListAsync();
        }

        public async static Task<int> GetRecordListCountAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
        {
            var par = context.GetType().GetProperty(dbSetName ?? IDbRecord<TRecord>.RecordName);
            var set = par.GetValue(context);
            var sets = (DbSet<TRecord>)set;
            return await sets.CountAsync();
        }

        public async static Task<TRecord> GetRecordAsync<TRecord>(this DbContext context, int id, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
        {
            var par = context.GetType().GetProperty(dbSetName ?? IDbRecord<TRecord>.RecordName);
            var set = par.GetValue(context);
            var sets = (DbSet<TRecord>)set;
            return await sets.FirstOrDefaultAsync(item => ((IDbRecord<TRecord>)item).ID == id);
        }

    }
}
