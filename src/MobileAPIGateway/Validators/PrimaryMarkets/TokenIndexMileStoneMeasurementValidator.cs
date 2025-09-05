using FluentValidation;
using MobileAPIGateway.Models.PrimaryMarkets;

namespace MobileAPIGateway.Validators.PrimaryMarkets;

/// <summary>
/// Validator for <see cref="TokenIndexMileStoneMeasurement"/>
/// </summary>
public class TokenIndexMileStoneMeasurementValidator : AbstractValidator<TokenIndexMileStoneMeasurement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenIndexMileStoneMeasurementValidator"/> class
    /// </summary>
    public TokenIndexMileStoneMeasurementValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.UnlockedMeasurement)
            .GreaterThanOrEqualTo(0)
            .When(x => x.UnlockedMeasurement.HasValue)
            .WithMessage("Unlocked measurement must be greater than or equal to 0");
    }
}
