using FluentValidation;
using MobileAPIGateway.Models.Users;

namespace MobileAPIGateway.Validators.Users;

/// <summary>
/// Validator for <see cref="UserTimeZone"/>
/// </summary>
public class UserTimeZoneValidator : AbstractValidator<UserTimeZone>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserTimeZoneValidator"/> class
    /// </summary>
    public UserTimeZoneValidator()
    {
        RuleFor(x => x.Zone)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Zone))
            .WithMessage("Time zone cannot exceed 100 characters");
    }
}
