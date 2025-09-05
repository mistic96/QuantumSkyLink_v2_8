using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="TeamPrice"/>
/// </summary>
public class TeamPriceValidator : AbstractValidator<TeamPrice>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TeamPriceValidator"/> class
    /// </summary>
    public TeamPriceValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");
            
        RuleFor(x => x.CurrentPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Current price must be greater than or equal to 0");
            
        RuleFor(x => x.PreviousPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Previous price must be greater than or equal to 0");
            
        RuleFor(x => x.PriceFluxPercentage!)
            .SetValidator(new PercentageFluxValidator())
            .When(x => x.PriceFluxPercentage != null);
            
        RuleFor(x => x.QuoteId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.QuoteId))
            .WithMessage("Quote ID cannot exceed 100 characters");
            
        RuleFor(x => x.BlockchainAlias)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.BlockchainAlias))
            .WithMessage("Blockchain alias cannot exceed 100 characters");
            
        RuleFor(x => x.FxRate!)
            .SetValidator(new FxRateValidator())
            .When(x => x.FxRate != null);
    }
}
