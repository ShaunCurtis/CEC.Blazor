using Microsoft.Extensions.Configuration;
using CEC.Blazor.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace CEC.Blazor.Server.Services
{
    public abstract class BaseDataService<TRecord>: IDataService<TRecord> where TRecord : new()
    {
        /// <summary>
        /// Access to the Application Configuration data
        /// </summary>
        public IConfiguration AppConfiguration { get; set; }

        /// <summary>
        /// Record Configuration data used by UI for display and navigation for records of type T
        /// </summary>
        public RecordConfigurationData RecordConfiguration { get; set; } = new RecordConfigurationData();

        public BaseDataService(IConfiguration configuration)
        {
            this.AppConfiguration = configuration;
        }

        /// <summary>
        /// Method to get a Database context to run an EF query on
        /// As most of what we are doing is Async we can't use a single context that we pass around
        /// </summary>
        /// <returns></returns>
        protected WeatherForecastDbContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<WeatherForecastDbContext>();

            optionsBuilder.UseSqlServer(AppConfiguration.GetConnectionString("WeatherForecastConnection"));
            return new WeatherForecastDbContext(optionsBuilder.Options);
        }

        // This would normally contain all the base boilerplate code for accessing the database context and doing CRUD operations
        // I'm old school and a little paranoid with data so link datasets to read only views for listing and viewing operations
        //  and use Stored Procedures and ExecuteSQLRawAsync for all CUD operations.

    }
}
