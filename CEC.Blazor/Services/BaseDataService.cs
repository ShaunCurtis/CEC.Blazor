using System;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using CEC.Blazor.Utilities;
using CEC.Blazor.Data;
using CEC.Blazor.Services;

namespace CEC.Blazor.Services
{
    public class BaseDataService<T>: IDbService<T> where T : new()
    {

        /// <summary>
        /// Access to the Application Configuration data
        /// </summary>
        public IConfiguration AppConfiguration { get; set; }

        /// <summary>
        /// Access to the Record Configuration data
        /// </summary>
        public RecordConfigurationData Configuration { get; set; } = new RecordConfigurationData();

        public BaseDataService(IConfiguration configuration)
        {
            this.AppConfiguration = configuration;
        }

        // This would nomrally contain all the base boilerplate code for accessing the database context and doing CRUD operations
        // I'm old school and a little paranoid with data so link datasets to read only views for listing and viewing operations
        //  and use Stored Procedures and ExecuteSQLRawAsync for all CUD operations.

    }
}
