using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEC.Weather.Services
{
    public class SalaryDataService
    {
        public List<EmployeeSalaryRecord> EmployeeSalaryRecords => new List<EmployeeSalaryRecord>() {
            new EmployeeSalaryRecord() { EmployeeID = 1, Salary = 250000, HasBonus = true },
            new EmployeeSalaryRecord() { EmployeeID = 2, Salary = 25000 }
        };

        public async Task<EmployeeSalaryRecord> GetEmployeeSalaryRecord(int EmployeeID)
        {
            // For Server
            // Example code to get a record though EF from a database
            // Note the use of FirstorDefaultAsync
            // return await mydatacontext.GetContext().EmployeeSalaryTable.FirstorDefaultAsync(item => item.EmployeeID == EmployeeID);
            // For WASM need another WASM version of the Data Service that calls an API on the Server which then plugs into this service
            //  Controller Service <--WASM Client--> WASM Data Service <===== Remote ======> API Controller <--API Server--> Server Data Service
            await Task.Delay(2000);
            // Substitute sync version that runs on a list
            return this.EmployeeSalaryRecords.First(item => item.EmployeeID == EmployeeID);
        }

    }

    public class EmployeeSalaryRecord
    {
        public int EmployeeID { get; set; } = 0;

        public decimal Salary { get; set; } = 0;

        public bool HasBonus { get; set; }
    }
}
