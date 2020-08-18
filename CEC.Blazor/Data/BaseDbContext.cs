using CEC.Blazor.Components;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;


namespace CEC.Blazor.Data
{
    public class BaseDbContext : DbContext
    {
        private DbContextOptions<DbContext> options;

        public BaseDbContext(DbContextOptions<DbContext> options)
        {
            this.options = options;
        }

        //public BaseDbContext(DbContextOptions<T> options) : base(options) { }

        public DbSet<DbID> IDs { get; set; }

        /// <summary>
        /// Async Method to run an stored procedure against the database context and generate a result object
        /// </summary>
        /// <param name="storedprocname"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected async Task<DbTaskResult> RunDatabaseStoredProcedureAsync(string storedprocname, List<SqlParameter> parameters, RecordConfigurationData recordConfiguration)
        {
            var rows = await this.Database.ExecuteSqlRawAsync(this.GetParameterizedSqlCommand(storedprocname, parameters));
            if (rows == -1) return new DbTaskResult() { 
                Message = $"{recordConfiguration.RecordDescription} saved", 
                IsOK = true, 
                Type = MessageType.Success 
            };
            else return new DbTaskResult() { 
                Message = $"Error saving {recordConfiguration.RecordDescription}", 
                IsOK = false, 
                Type = MessageType.Error
            };
        }

        /// <summary>
        /// Async Method to run an stored procedure against the database context and generate a result object
        /// </summary>
        /// <param name="storedprocname"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected async Task<DbTaskResult> RunDatabaseIDStoredProcedureAsync(string storedprocname, List<SqlParameter> parameters, RecordConfigurationData recordConfiguration)
        {
            int id = 0;

            var rows = this.IDs.FromSqlRaw(this.GetParameterizedSqlCommand(storedprocname, parameters)).AsAsyncEnumerable();
            await foreach (var row in rows) id = Convert.ToInt32(row.Id);
            if (id > 0) return new DbTaskResult()
            {
                Message = $"{recordConfiguration.RecordDescription} saved",
                IsOK = true,
                Type = MessageType.Success
            };
            else return new DbTaskResult()
            {
                Message = $"Error saving {recordConfiguration.RecordDescription}",
                IsOK = false,
                Type = MessageType.Error
            };
        }

        /// <summary>
        /// Method to get a parametized SQL Command from a list of parameters
        /// Do this as the FromSQLRaw doesn't take a list of parameters correctly, so we need to generate the list for it
        /// </summary>
        /// <param name="storedprocname"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected string GetParameterizedSqlCommand(string storedprocname, List<SqlParameter> parameters)
        {
            var paramstring = new StringBuilder();
            var quotedtypes = new List<SqlDbType>() { SqlDbType.NVarChar, SqlDbType.Char, SqlDbType.NChar, SqlDbType.NText, SqlDbType.Text, SqlDbType.VarChar };
            var datetypes = new List<SqlDbType>() { SqlDbType.Date, SqlDbType.DateTime, SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.SmallDateTime };

            foreach (var par in parameters)
            {
                if (paramstring.Length > 0) paramstring.Append(", ");
                if (quotedtypes.Contains(par.SqlDbType)) paramstring.Append(string.Concat(par.ParameterName, "='", par.Value, "'"));
                else if (datetypes.Contains(par.SqlDbType)) paramstring.Append(string.Concat(par.ParameterName, "='", par.Value, "'"));
                else paramstring.Append(string.Concat(par.ParameterName, "=", par.Value));
            }
            return $"exec {storedprocname} {paramstring}";
        }
    }
}
