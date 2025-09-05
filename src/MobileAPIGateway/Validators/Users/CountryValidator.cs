using FluentValidation;
using MobileAPIGateway.Models.Users;

namespace MobileAPIGateway.Validators.Users;

/// <summary>
/// Validator for <see cref="Country"/>
/// </summary>
public class CountryValidator : AbstractValidator<Country>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountryValidator"/> class
    /// </summary>
    public CountryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Country name cannot exceed 100 characters");
            
        RuleFor(x => x.Alpha2Code)
            .Length(2).When(x => !string.IsNullOrEmpty(x.Alpha2Code))
            .WithMessage("Alpha-2 code must be exactly 2 characters");
            
        RuleFor(x => x.Alpha3Code)
            .Length(3).When(x => !string.IsNullOrEmpty(x.Alpha3Code))
            .WithMessage("Alpha-3 code must be exactly 3 characters");
            
        RuleFor(x => x.CallingCode)
            .MaximumLength(10).When(x => !string.IsNullOrEmpty(x.CallingCode))
            .WithMessage("Calling code cannot exceed 10 characters");
            
        RuleFor(x => x.Flag)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Flag))
            .WithMessage("Flag URL cannot exceed 255 characters");
            
        RuleForEach(x => x.TimeZones)
            .SetValidator(new UserTimeZoneValidator())
            .When(x => x.TimeZones != null);
    }
}
