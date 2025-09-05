using FluentValidation;
using MobileAPIGateway.Models.CustomerMarkets;

namespace MobileAPIGateway.Validators.CustomerMarkets;

/// <summary>
/// Validator for the customer market model
/// </summary>
public class CustomerMarketValidator : AbstractValidator<CustomerMarket>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerMarketValidator"/> class
    /// </summary>
    public CustomerMarketValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer market ID is required");
        
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
        
        RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("Market ID is required");
        
        RuleFor(x => x.MarketName)
            .NotEmpty().WithMessage("Market name is required")
            .MaximumLength(100).WithMessage("Market name cannot exceed 100 characters");
        
        RuleFor(x => x.MarketDescription)
            .MaximumLength(1000).WithMessage("Market description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.MarketDescription));
        
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
        
        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tag cannot be empty")
            .MaximumLength(50).WithMessage("Tag cannot exceed 50 characters");
        
        RuleFor(x => x.CreatedAt)
            .NotEmpty().WithMessage("Creation date is required");
        
        RuleFor(x => x.UpdatedAt)
            .NotEmpty().WithMessage("Update date is required");
        
        RuleFor(x => x.SubscriptionExpiresAt)
            .GreaterThan(x => x.CreatedAt).WithMessage("Subscription expiration date must be after creation date")
            .When(x => x.SubscriptionExpiresAt.HasValue);
        
        RuleForEach(x => x.TradingPairs)
            .SetValidator(new CustomerTradingPairValidator());
    }
}
