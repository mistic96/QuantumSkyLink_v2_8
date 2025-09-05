using FluentValidation;
using MobileAPIGateway.Models.Markets;

namespace MobileAPIGateway.Validators.Markets;

/// <summary>
/// Validator for the trading pair model
/// </summary>
public class TradingPairValidator : AbstractValidator<TradingPair>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TradingPairValidator"/> class
    /// </summary>
    public TradingPairValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Trading pair ID is required");
        
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Trading pair symbol is required")
            .MaximumLength(20).WithMessage("Trading pair symbol cannot exceed 20 characters");
        
        RuleFor(x => x.BaseAsset)
            .NotEmpty().WithMessage("Base asset is required")
            .MaximumLength(20).WithMessage("Base asset cannot exceed 20 characters");
        
        RuleFor(x => x.QuoteAsset)
            .NotEmpty().WithMessage("Quote asset is required")
            .MaximumLength(20).WithMessage("Quote asset cannot exceed 20 characters");
        
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Trading pair status is required")
            .MaximumLength(50).WithMessage("Trading pair status cannot exceed 50 characters");
        
        RuleFor(x => x.MinOrderSize)
            .GreaterThan(0).WithMessage("Minimum order size must be greater than 0");
        
        RuleFor(x => x.MaxOrderSize)
            .GreaterThan(x => x.MinOrderSize).WithMessage("Maximum order size must be greater than minimum order size");
        
        RuleFor(x => x.PricePrecision)
            .GreaterThanOrEqualTo(0).WithMessage("Price precision must be greater than or equal to 0");
        
        RuleFor(x => x.QuantityPrecision)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity precision must be greater than or equal to 0");
        
        RuleFor(x => x.BaseAssetPrecision)
            .GreaterThanOrEqualTo(0).WithMessage("Base asset precision must be greater than or equal to 0");
        
        RuleFor(x => x.QuoteAssetPrecision)
            .GreaterThanOrEqualTo(0).WithMessage("Quote asset precision must be greater than or equal to 0");
        
        RuleFor(x => x.TradingFee)
            .GreaterThanOrEqualTo(0).WithMessage("Trading fee must be greater than or equal to 0");
    }
}
