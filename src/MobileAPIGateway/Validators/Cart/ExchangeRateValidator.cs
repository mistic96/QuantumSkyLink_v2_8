using FluentValidation;
using MobileAPIGateway.Models.Cart;

namespace MobileAPIGateway.Validators.Cart;

/// <summary>
/// Validator for <see cref="ExchangeRate"/>
/// </summary>
public class ExchangeRateValidator : AbstractValidator<ExchangeRate>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExchangeRateValidator"/> class
    /// </summary>
    public ExchangeRateValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID must be greater than 0");
            
        RuleFor(x => x.CurrencyName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.CurrencyName))
            .WithMessage("Currency name cannot exceed 50 characters");
            
        RuleFor(x => x.CurrencyRate)
            .GreaterThan(0)
            .When(x => x.CurrencyRate.HasValue)
            .WithMessage("Currency rate must be greater than 0");
            
        RuleFor(x => x.UsdValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("USD value must be greater than or equal to 0");
            
        RuleFor(x => x.QuoteId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.QuoteId))
            .WithMessage("Quote ID cannot exceed 100 characters");
    }
}
