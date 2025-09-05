using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="PercentageFlux"/>
/// </summary>
public class PercentageFluxValidator : AbstractValidator<PercentageFlux>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PercentageFluxValidator"/> class
    /// </summary>
    public PercentageFluxValidator()
    {
        RuleFor(x => x.Result)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Result must be greater than or equal to 0");
    }
}
