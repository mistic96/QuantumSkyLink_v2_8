using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="EstimatedProfitProjection"/>
/// </summary>
public class EstimatedProfitProjectionValidator : AbstractValidator<EstimatedProfitProjection>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EstimatedProfitProjectionValidator"/> class
    /// </summary>
    public EstimatedProfitProjectionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.ExchangeRate!)
            .SetValidator(new LedgerMarketExchangeRateValidator())
            .When(x => x.ExchangeRate != null);
    }
}
