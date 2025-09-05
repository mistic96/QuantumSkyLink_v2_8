using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="FxRate"/>
/// </summary>
public class FxRateValidator : AbstractValidator<FxRate>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FxRateValidator"/> class
    /// </summary>
    public FxRateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.QuoteId)
            .NotEmpty()
            .WithMessage("Quote ID is required");
            
        RuleFor(x => x.Rate)
            .GreaterThan(0)
            .WithMessage("Rate must be greater than 0");
            
        RuleFor(x => x.BaseExchangeRate)
            .GreaterThan(0)
            .WithMessage("Base exchange rate must be greater than 0");
            
        RuleFor(x => x.PayoutExchangeRate)
            .GreaterThan(0)
            .WithMessage("Payout exchange rate must be greater than 0");
    }
}
