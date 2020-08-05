using Microsoft.Extensions.Configuration;
using CEC.Blazor.Data;

namespace CEC.Blazor.Services
{
    public class BaseDataService<TRecord>: IDataService<TRecord> where TRecord : new()
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

        // This would normally contain all the base boilerplate code for accessing the database context and doing CRUD operations
        // I'm old school and a little paranoid with data so link datasets to read only views for listing and viewing operations
        //  and use Stored Procedures and ExecuteSQLRawAsync for all CUD operations.

    }
}
