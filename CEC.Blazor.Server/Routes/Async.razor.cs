using CEC.Blazor.Server.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Routes
{
    public partial class Async : ComponentBase, IDisposable
    {

        [Inject]
        protected SalaryControllerService SalaryControllerService { get; set; }

        protected string Message => SalaryControllerService?.Message ?? string.Empty;

        protected decimal Salary { get; set; }

        protected RenderFragment DeadlockButton { get; set; }

        protected RenderFragment BossButton { get; set; }

        protected RenderFragment MyButton { get; set; }

        protected override Task OnInitializedAsync()
        {
            this.SalaryControllerService.MessageChanged += this.MessageUpdated;
            this.BossButton = BuildButton(1, false);
            this.MyButton = BuildButton(2, false);
            this.DeadlockButton = BuildButton(0, false);
            return Task.CompletedTask;
        }

        public void Dispose() => this.SalaryControllerService.MessageChanged += this.MessageUpdated;

        protected void MessageUpdated(object sender, EventArgs e) => InvokeAsync(this.StateHasChanged);

        public async void ButtonClicked(int employeeID)
        {
            this.SortButtons(employeeID, true);
            this.Salary = await this.SalaryControllerService.GetEmployeeSalary(employeeID, 3);
            this.SortButtons(employeeID, false);
            await InvokeAsync(this.StateHasChanged);
        }

        public async void UnsafeButtonClicked()
        {
            this.DeadlockButton = BuildButton(0, true);
            this.SalaryControllerService.Message = "You clicked it!";
            await Task.Delay(2000);
            await InvokeAsync(this.StateHasChanged);
            var x = this.SalaryControllerService.BlockerTask().Result;
            this.DeadlockButton = BuildButton(0, true);
            await InvokeAsync(this.StateHasChanged);
        }

        protected void SortButtons(int employeeID,bool running)
        {
            if (employeeID == 1) this.BossButton = BuildButton(employeeID, running);
            else this.MyButton = BuildButton(employeeID, running);
        }

        protected RenderFragment BuildButton(int Id, bool running)
        {
            string css;
            string label;

            switch (Id)
            {
                case 0:
                    css = "btn-danger";
                    label = "Road to Deadlock";
                    break;
                case 1:
                    css = "btn-warning";
                    label = "Boss's Salary";
                    break;
                default:
                    css = "btn-primary";
                    label = "My Salary";
                    break;
            }
            var value = running ? "   Running..." : label;
            var i = -1;
            RenderFragment button = builder =>
            {
                builder.OpenElement(i++, "button");
                builder.AddAttribute(i++, "class", $"btn {css}");
                builder.AddAttribute(i++, "type", "button");
                if (Id > 0) builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => this.ButtonClicked(Id)));
                else builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => this.UnsafeButtonClicked()));
                if (running)
                {
                    builder.AddAttribute(i++, "disabled", "disabled");
                    builder.OpenElement(i++, "span");
                    builder.AddAttribute(i++, "class", "spinner-border spinner-border-sm pr-2");
                    builder.AddAttribute(i++, "role", "status");
                    builder.AddAttribute(i++, "aria-hidden", "true");
                    builder.CloseElement();
                }
                builder.AddContent(i++, value);
                builder.CloseElement();

            };
            return button;
        }
    }
}
