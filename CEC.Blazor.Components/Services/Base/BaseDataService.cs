using Microsoft.Extensions.Configuration;
using CEC.Blazor.Data;
using CEC.Blazor.Services;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;

namespace CEC.Blazor.Services
{
    public abstract class BaseDataService<TRecord, TContext>: IDataService<TRecord, TContext>
        where TRecord : class, new()
        where TContext : DbContext
    {
        /// <summary>
        /// Access to the HttpClient
        /// </summary>
        public HttpClient HttpClient { get; set; } = null;

        public virtual IDbContextFactory<TContext> DBContext { get; set; } = null;

        /// <summary>
        /// Access to the Application Configuration data
        /// </summary>
        public IConfiguration AppConfiguration { get; set; }

        /// <summary>
        /// Record Configuration data used by UI for display and navigation for records of type T
        /// </summary>
        public virtual RecordConfigurationData RecordConfiguration { get; set; } = new RecordConfigurationData();

        public BaseDataService(IConfiguration configuration) => this.AppConfiguration = configuration;

    }
}
