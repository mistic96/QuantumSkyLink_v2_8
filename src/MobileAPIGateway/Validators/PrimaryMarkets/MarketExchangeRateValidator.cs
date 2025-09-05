using FluentValidation;
using MobileAPIGateway.Models.PrimaryMarkets;

namespace MobileAPIGateway.Validators.PrimaryMarkets;

/// <summary>
/// Validator for <see cref="MarketExchangeRate"/>
/// </summary>
public class MarketExchangeRateValidator : AbstractValidator<MarketExchangeRate>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarketExchangeRateValidator"/> class
    /// </summary>
    public MarketExchangeRateValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID must be greater than 0");
            
        RuleFor(x => x.CurrencyRate)
            .GreaterThan(0)
            .When(x => x.CurrencyRate.HasValue)
            .WithMessage("Currency rate must be greater than 0");
            
        RuleFor(x => x.PriceInUsd)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price in USD must be greater than or equal to 0");
            
        RuleFor(x => x.QuoteId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.QuoteId))
            .WithMessage("Quote ID cannot exceed 100 characters");
    }
}
