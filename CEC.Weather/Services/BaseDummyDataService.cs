using Microsoft.Extensions.Configuration;
using CEC.Blazor.Data;
using CEC.Blazor.Services;

namespace CEC.Weather.Services
{
    public abstract class BaseDummyDataService<TRecord>: IDataService<TRecord> where TRecord : new()
    {
        /// <summary>
        /// Access to the Application Configuration data
        /// </summary>
        public IConfiguration AppConfiguration { get; set; }

        /// <summary>
        /// Record Configuration data used by UI for display and navigation for records of type T
        /// </summary>
        public RecordConfigurationData RecordConfiguration { get; set; } = new RecordConfigurationData();

        public BaseDummyDataService(IConfiguration configuration)
        {
            this.AppConfiguration = configuration;
        }

    }
}
