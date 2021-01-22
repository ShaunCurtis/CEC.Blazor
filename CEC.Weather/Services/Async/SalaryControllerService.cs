using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEC.Weather.Services
{
    public class SalaryControllerService
    {
        public SalaryDataService SalaryDataService { get; set; }

        public event EventHandler MessageChanged;

        public string Message { get; set; } = string.Empty;

        public SalaryControllerService(SalaryDataService salaryDataService)
        {
            this.SalaryDataService = salaryDataService;
        }

        public async Task<decimal> GetEmployeeSalary(int employeeID, int egorating)
        {
            this.Message = "Getting Employee record";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            var rec = await this.SalaryDataService.GetEmployeeSalaryRecord(employeeID);
            if (rec.HasBonus)
            {
                this.Message = "Wow big bonus to calculate - this could take a while!";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
                var bonus = await Task.Run(() => this.CalculateBossesBonus(egorating));
                this.Message = "Overpaid git!";
                return rec.Salary + bonus;
            }
            else
            {
                this.Message = "You need a rise!";
                return rec?.Salary ?? 0m;
            }
        }

        private async Task<decimal> CalculateBossesBonus(int egorating)
        {
            // it takes a long while to do this calculation
            await Task.Delay(1000 * egorating);
            return 100000m * egorating;
        }

        public async Task<bool> BlockerTask()
        {
            this.Message = "That's blown it.  F5 to get out of this one.";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(1000);
            return true;

        }

    }
}
