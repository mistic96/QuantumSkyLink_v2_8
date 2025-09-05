using FluentValidation;
using MobileAPIGateway.Models.Markets;

namespace MobileAPIGateway.Validators.Markets;

/// <summary>
/// Validator for the market model
/// </summary>
public class MarketValidator : AbstractValidator<Market>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarketValidator"/> class
    /// </summary>
    public MarketValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Market ID is required");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Market name is required")
            .MaximumLength(100).WithMessage("Market name cannot exceed 100 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Market description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
        
        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));
        
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Market status is required")
            .MaximumLength(50).WithMessage("Market status cannot exceed 50 characters");
        
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Market type is required")
            .MaximumLength(50).WithMessage("Market type cannot exceed 50 characters");
        
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Market category is required")
            .MaximumLength(50).WithMessage("Market category cannot exceed 50 characters");
        
        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price must be greater than or equal to 0");
        
        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tag cannot be empty")
            .MaximumLength(50).WithMessage("Tag cannot exceed 50 characters");
        
        RuleForEach(x => x.TradingPairs)
            .SetValidator(new TradingPairValidator());
        
        When(x => x.PricingStrategy == PricingStrategy.Tiered, () =>
        {
            RuleFor(x => x.PriceTiers)
                .NotNull().WithMessage("Price tiers are required for tiered pricing strategy")
                .NotEmpty().WithMessage("Price tiers cannot be empty for tiered pricing strategy");
            
            RuleForEach(x => x.PriceTiers)
                .SetValidator(new PriceTierValidator());
        });
    }
}
