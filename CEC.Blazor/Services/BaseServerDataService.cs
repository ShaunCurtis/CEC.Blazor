using CEC.Blazor.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using CEC.Blazor.Extensions;
using CEC.Blazor.Components;
using Microsoft.Extensions.Configuration;

namespace CEC.Blazor.Services
{
    public abstract class BaseServerDataService<TRecord, TContext> : 
        BaseDataService<TRecord, TContext>, 
        IDataService<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext
    {

        /// <summary>
        /// Property that contains the Database Access Data Associated with the Record
        /// </summary>
        protected DbRecordInfo RecordInfo { get; set; } = new DbRecordInfo();

        public BaseServerDataService(IConfiguration configuration, IDbContextFactory<TContext> dbContext) : base(configuration)
        {
            this.DBContext = dbContext;
            this.GetSPParameterAttributes();
            this.GetDbAccessAttribute();
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<List<TRecord>> GetRecordListAsync() => await this.DBContext.CreateDbContext().GetRecordListAsync<TRecord>();

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<List<TRecord>> GetFilteredRecordListAsync(IFilterList filterList) => await this.DBContext.CreateDbContext().GetRecordFilteredListAsync<TRecord>(filterList);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<TRecord> GetRecordAsync(int id) => await this.DBContext.CreateDbContext().GetRecordAsync<TRecord>(id);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetRecordListCountAsync() => await this.DBContext.CreateDbContext().GetRecordListCountAsync<TRecord>();

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> UpdateRecordAsync(TRecord record) => await this.RunStoredProcedure(record, SPType.Update);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> CreateRecordAsync(TRecord record) => await this.RunStoredProcedure(record, SPType.Create);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> DeleteRecordAsync(TRecord record) => await this.RunStoredProcedure(record, SPType.Delete);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public async Task<List<string>> GetDistinctListAsync(DbDistinctRequest req) => await this.DBContext.CreateDbContext().GetDistinctListAsync(req);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public async Task<List<DbBaseRecord>> GetBaseRecordListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup> => await this.DBContext.CreateDbContext().GetBaseRecordListAsync<TLookup>();

        /// <summary>
        /// Method to Check if a property has SPParameter Attribute
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool HasSPParameterAttribute(string propertyName)
        {
            var prop = typeof(TRecord).GetProperty(propertyName);
            var attrs = prop.GetCustomAttributes(true);
            var attribute = (SPParameterAttribute)attrs.FirstOrDefault(item => item.GetType() == typeof(SPParameterAttribute));
            return attribute != null;
        }

        /// <summary>
        /// Method to get a SPParameter Attribute
        /// </summary>
        protected void GetSPParameterAttributes()
        {
            var attrprops = new List<PropertyInfo>();
            foreach (var prop in typeof(TRecord).GetProperties())
            {
                if (this.HasSPParameterAttribute(prop.Name)) attrprops.Add(prop);
            }
            RecordInfo.SPProperties = attrprops;
        }

        /// <summary>
        /// Method to get the DbAccessAttribute for this class
        /// </summary>
        protected void GetDbAccessAttribute()
        {
            var attrs = typeof(TRecord).GetCustomAttributes(true);
            var classname = typeof(TRecord).Name.Replace("Db", "");
            var attribute = (DbAccessAttribute)attrs.FirstOrDefault(item => item.GetType() == typeof(DbAccessAttribute));
            RecordInfo.CreateSP = attribute is null || string.IsNullOrEmpty(attribute.CreateSP) ? $"sp_Create_{classname}" : attribute.CreateSP;
            RecordInfo.DeleteSP = attribute is null || string.IsNullOrEmpty(attribute.DeleteSP) ? $"sp_Delete_{classname}" : attribute.DeleteSP;
            RecordInfo.UpdateSP = attribute is null || string.IsNullOrEmpty(attribute.UpdateSP) ? $"sp_Update_{classname}" : attribute.UpdateSP;
            RecordInfo.RecordName = attribute is null || string.IsNullOrEmpty(attribute.RecordName) ? $"{classname}" : attribute.RecordName;
        }

        /// <summary>
        /// Method to execute a stored procedure against the dataservice
        /// </summary>
        /// <param name="record"></param>
        /// <param name="spType"></param>
        /// <returns></returns>
        protected async Task<DbTaskResult> RunStoredProcedure(TRecord record, SPType spType)
        {
            var ret = new DbTaskResult()
            {
                Message = $"Error saving {this.RecordConfiguration.RecordDescription}",
                IsOK = false,
                Type = MessageType.Error
            };

            var spname = spType switch
            {
                SPType.Create => this.RecordInfo.CreateSP,
                SPType.Update => this.RecordInfo.UpdateSP,
                SPType.Delete => this.RecordInfo.DeleteSP,
                _ => string.Empty
            };
            var parms = this.GetSQLParameters(record, spType);
            if (await this.DBContext.CreateDbContext().ExecStoredProcAsync(spname, parms))
            {
                var idparam = parms.FirstOrDefault(item => item.Direction == ParameterDirection.Output && item.SqlDbType == SqlDbType.Int && item.ParameterName.Contains("ID"));
                ret = new DbTaskResult()
                {
                    Message = $"{this.RecordConfiguration.RecordDescription} saved",
                    IsOK = true,
                    Type = MessageType.Success
                };
                if (idparam != null) ret.NewID = Convert.ToInt32(idparam.Value);
            }
            return ret;
        }

        /// <summary>
        /// Method that sets up the SQL Stored Procedure Parameters
        /// </summary>
        /// <param name="item"></param>
        /// <param name="withid"></param>
        /// <returns></returns>
        protected virtual List<SqlParameter> GetSQLParameters(TRecord record, SPType spType)
        {
            var parameters = new List<SqlParameter>();
            foreach (var prop in RecordInfo.SPProperties)
            {
                var attr = prop.GetCustomAttribute<SPParameterAttribute>();
                attr.CheckName(prop);
                // If its a delete we only need the ID and then break out of the for
                if (attr.IsID && spType == SPType.Delete)
                {
                    parameters.Add(new SqlParameter(attr.ParameterName, attr.DataType) { Direction = ParameterDirection.Input, Value = prop.GetValue(record) });
                    break;
                }
                // skip if its a delete
                if (spType != SPType.Delete)
                {
                    // if its a create add the ID as an output foe capturing the new ID
                    if (attr.IsID && spType == SPType.Create) parameters.Add(new SqlParameter(attr.ParameterName, attr.DataType) { Direction = ParameterDirection.Output });
                    // Deal with dates
                    else if (attr.DataType == SqlDbType.SmallDateTime) parameters.Add(new SqlParameter(attr.ParameterName, attr.DataType) { Direction = ParameterDirection.Input, Value = ((DateTime)prop.GetValue(record)).ToString("dd-MMM-yyyy") });
                    else parameters.Add(new SqlParameter(attr.ParameterName, attr.DataType) { Direction = ParameterDirection.Input, Value = prop.GetValue(record) });
                }
            }
            return parameters;
        }
    }
}
