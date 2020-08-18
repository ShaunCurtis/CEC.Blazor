using CEC.Blazor.Server.Data;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CEC.Blazor.Server.Data.Validators
{
    public class WeatherForecastValidator : AbstractValidator<DbWeatherForecast>
    {
        public WeatherForecastValidator()
        {
            RuleFor(p => p.Date).NotEmpty().WithMessage("You must select a date");
            RuleFor(p => p.TemperatureC).LessThan(60).WithMessage("The temperature must be less than 60C");
            RuleFor(p => p.TemperatureC).GreaterThan(-40).WithMessage("The temperature must be greater than -40C");
            RuleFor(p => p.PostCode).Cascade(CascadeMode.StopOnFirstFailure).NotEmpty().Matches(@"^([A-PR-UWYZ0-9][A-HK-Y0-9][AEHMNPRTVXY0-9]?[ABEHMNPRVWXY0-9]? {1,2}[0-9][ABD-HJLN-UW-Z]{2}|GIR 0AA)$").WithMessage("You must enter a Post Code (e.g. GL52 8BX)");
            RuleFor(p => p.Description).MinimumLength(6).WithMessage("Your description needs to be a little longer!");
        }
    }
}
