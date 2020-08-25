using CEC.Blazor.Components;
using CEC.Blazor.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Data
{
    public class WeatherForecastDbContext : DbContext
    {
        public WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> options) : base(options) { }

        public DbSet<DbWeatherForecast> WeatherForecasts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<DbWeatherForecast>(eb =>
                {
                    eb.HasNoKey();
                    eb.ToView("vw_WeatherForecasts");
                });
        }

        /// <summary>
        /// Async Method to run an stored procedure against the database context and generate a result object
        /// </summary>
        /// <param name="storedprocname"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        internal async Task<DbTaskResult> RunStoredProcedureAsync(string storedprocname, List<SqlParameter> parameters, RecordConfigurationData recordConfiguration)
        {
            var rows = await this.Database.ExecuteSqlRawAsync(this.GetParameterizedNames(storedprocname, parameters), parameters);
            if (rows == 1)
            {
                var idparam = parameters.FirstOrDefault(item => item.Direction == ParameterDirection.Output && item.SqlDbType == SqlDbType.Int && item.ParameterName.Contains("ID"));
                var ret = new DbTaskResult()
                {
                    Message = $"{recordConfiguration.RecordDescription} saved",
                    IsOK = true,
                    Type = MessageType.Success
                };
                if (idparam != null) ret.NewID = Convert.ToInt32(idparam.Value);
                return ret;
            }
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
        protected string GetParameterizedNames(string storedprocname, List<SqlParameter> parameters)
        {
            var paramstring = new StringBuilder();

            foreach (var par in parameters)
            {
                if (paramstring.Length > 0) paramstring.Append(", ");
                if (par.Direction == ParameterDirection.Output) paramstring.Append($"{par.ParameterName} output");
                else paramstring.Append(par.ParameterName);
            }
            return $"exec {storedprocname} {paramstring}";
        }

    }
}
