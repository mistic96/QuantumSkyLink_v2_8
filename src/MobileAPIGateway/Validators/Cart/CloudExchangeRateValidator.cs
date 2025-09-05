using FluentValidation;
using MobileAPIGateway.Models.Cart;

namespace MobileAPIGateway.Validators.Cart;

/// <summary>
/// Validator for <see cref="CloudExchangeRate"/>
/// </summary>
public class CloudExchangeRateValidator : AbstractValidator<CloudExchangeRate>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudExchangeRateValidator"/> class
    /// </summary>
    public CloudExchangeRateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
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
