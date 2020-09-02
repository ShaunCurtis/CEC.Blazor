using Microsoft.Extensions.Configuration;
using CEC.Blazor.Data;
using CEC.Blazor.Services;
using CEC.Weather.Data;
using Microsoft.EntityFrameworkCore;

namespace CEC.Weather.Services
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

            //var connectionstring = AppConfiguration.GetConnectionString("WeatherForecastConnection");
            var connectionstring = "Server=(localdb)\\mssqllocaldb;Database=WeatherForecast;Trusted_Connection=True;MultipleActiveResultSets=true";
            optionsBuilder.UseSqlServer(connectionstring);
            return new WeatherForecastDbContext(optionsBuilder.Options);
        }
    }
}
