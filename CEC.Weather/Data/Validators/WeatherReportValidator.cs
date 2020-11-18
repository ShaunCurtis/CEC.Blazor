using FluentValidation;

namespace CEC.Weather.Data.Validators
{
    public class WeatherReportValidator : AbstractValidator<DbWeatherReport>
    {
        public WeatherReportValidator()
        {
            RuleFor(p => p.Date).NotEmpty().WithMessage("You must select a date");
            RuleFor(p => p.TempMax).LessThan(60).WithMessage("The temperature must be less than 60C");
            RuleFor(p => p.TempMax).GreaterThan(-40).WithMessage("The temperature must be greater than -40C");
            RuleFor(p => p.TempMin).LessThan(60).WithMessage("The temperature must be less than 60C");
            RuleFor(p => p.TempMin).GreaterThan(-40).WithMessage("The temperature must be greater than -40C");
            RuleFor(p => p.FrostDays).LessThan(32).WithMessage("There's a maximun of 31 days in any month");
            RuleFor(p => p.FrostDays).GreaterThan(0).WithMessage("valid entries are 0-31");
            RuleFor(p => p.Rainfall).GreaterThan(0).WithMessage("valid entries are 0-31");
            RuleFor(p => p.SunHours).LessThan(24).WithMessage("Valid entries 0-24");
            RuleFor(p => p.SunHours).GreaterThan(0).WithMessage("Valid entries 0-24");
        }
    }
}
