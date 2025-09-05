using FluentValidation;
using MobileAPIGateway.Models.CustomerMarkets;

namespace MobileAPIGateway.Validators.CustomerMarkets;

/// <summary>
/// Validator for the customer market subscription plan model
/// </summary>
public class CustomerMarketSubscriptionPlanValidator : AbstractValidator<CustomerMarketSubscriptionPlan>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerMarketSubscriptionPlanValidator"/> class
    /// </summary>
    public CustomerMarketSubscriptionPlanValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Subscription plan ID is required");
        
        RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("Market ID is required");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subscription plan name is required")
            .MaximumLength(100).WithMessage("Subscription plan name cannot exceed 100 characters");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Subscription plan description is required")
            .MaximumLength(1000).WithMessage("Subscription plan description cannot exceed 1000 characters");
        
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");
        
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(10).WithMessage("Currency cannot exceed 10 characters");
        
        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Duration days must be greater than 0");
        
        RuleForEach(x => x.Features)
            .NotEmpty().WithMessage("Feature cannot be empty")
            .MaximumLength(100).WithMessage("Feature cannot exceed 100 characters");
        
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Subscription plan status is required")
            .MaximumLength(50).WithMessage("Subscription plan status cannot exceed 50 characters");
        
        RuleFor(x => x.CreatedAt)
            .NotEmpty().WithMessage("Creation date is required");
        
        RuleFor(x => x.UpdatedAt)
            .NotEmpty().WithMessage("Update date is required");
        
        RuleFor(x => x.MaxTradingPairs)
            .GreaterThan(0).WithMessage("Maximum trading pairs must be greater than 0")
            .When(x => x.MaxTradingPairs.HasValue);
        
        RuleFor(x => x.MaxAlerts)
            .GreaterThan(0).WithMessage("Maximum alerts must be greater than 0")
            .When(x => x.MaxAlerts.HasValue);
    }
}
