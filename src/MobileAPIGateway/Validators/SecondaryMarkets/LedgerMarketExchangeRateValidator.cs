using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="LedgerMarketExchangeRate"/>
/// </summary>
public class LedgerMarketExchangeRateValidator : AbstractValidator<LedgerMarketExchangeRate>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LedgerMarketExchangeRateValidator"/> class
    /// </summary>
    public LedgerMarketExchangeRateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.ExchangeRate)
            .GreaterThan(0)
            .WithMessage("Exchange rate must be greater than 0");
            
        RuleFor(x => x.UsdValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("USD value must be greater than or equal to 0");
            
        RuleFor(x => x.QuoteId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.QuoteId))
            .WithMessage("Quote ID cannot exceed 100 characters");
    }
}
