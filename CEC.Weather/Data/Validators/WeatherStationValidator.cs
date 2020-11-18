using FluentValidation;

namespace CEC.Weather.Data.Validators
{
    public class WeatherStationValidator : AbstractValidator<DbWeatherStation>
    {
        public WeatherStationValidator()
        {
            RuleFor(p => p.Longitude).LessThan(-180).WithMessage("Longitude must be -180 or greater");
            RuleFor(p => p.Longitude).GreaterThan(180).WithMessage("Longitude must be 180 or less");
            RuleFor(p => p.Latitude).LessThan(-90).WithMessage("Latitude must be -90 or greater");
            RuleFor(p => p.Latitude).GreaterThan(90).WithMessage("Latitude must be 90 or less");
            RuleFor(p => p.Name).MinimumLength(1).WithMessage("Your need a Station Name!");
        }
    }
}
