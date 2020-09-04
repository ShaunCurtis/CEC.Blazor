using Microsoft.Extensions.Configuration;
using CEC.Blazor.Data;
using CEC.Blazor.Services;
using System.Net.Http;

namespace CEC.Blazor.Services
{
    public abstract class BaseDummyDataService<TRecord>: IDataService<TRecord> where TRecord : new()
    {
        /// <summary>
        /// Access to the HttpClient
        /// </summary>
        public HttpClient HttpClient { get; set; }

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
