using FluentValidation;
using MobileAPIGateway.Models.Markets;

namespace MobileAPIGateway.Validators.Markets;

/// <summary>
/// Validator for the trade model
/// </summary>
public class TradeValidator : AbstractValidator<Trade>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TradeValidator"/> class
    /// </summary>
    public TradeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Trade ID is required");
        
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Trading pair symbol is required")
            .MaximumLength(20).WithMessage("Trading pair symbol cannot exceed 20 characters");
        
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
        
        RuleFor(x => x.QuoteQuantity)
            .GreaterThan(0).WithMessage("Quote quantity must be greater than 0");
        
        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required");
    }
}
