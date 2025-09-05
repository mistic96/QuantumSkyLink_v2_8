using FluentValidation;
using MobileAPIGateway.Models.CustomerMarkets;

namespace MobileAPIGateway.Validators.CustomerMarkets;

/// <summary>
/// Validator for the customer market subscription model
/// </summary>
public class CustomerMarketSubscriptionValidator : AbstractValidator<CustomerMarketSubscription>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerMarketSubscriptionValidator"/> class
    /// </summary>
    public CustomerMarketSubscriptionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Subscription ID is required");
        
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
        
        RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("Market ID is required");
        
        RuleFor(x => x.SubscriptionPlanId)
            .NotEmpty().WithMessage("Subscription plan ID is required");
        
        RuleFor(x => x.SubscriptionPlanName)
            .NotEmpty().WithMessage("Subscription plan name is required")
            .MaximumLength(100).WithMessage("Subscription plan name cannot exceed 100 characters");
        
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Subscription status is required")
            .MaximumLength(50).WithMessage("Subscription status cannot exceed 50 characters");
        
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");
        
        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
        
        RuleFor(x => x.RenewalDate)
            .GreaterThanOrEqualTo(x => x.EndDate).WithMessage("Renewal date must be after or equal to end date")
            .When(x => x.RenewalDate.HasValue);
        
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");
        
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(10).WithMessage("Currency cannot exceed 10 characters");
        
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .MaximumLength(50).WithMessage("Payment method cannot exceed 50 characters");
        
        RuleFor(x => x.PaymentStatus)
            .NotEmpty().WithMessage("Payment status is required")
            .MaximumLength(50).WithMessage("Payment status cannot exceed 50 characters");
        
        RuleFor(x => x.CreatedAt)
            .NotEmpty().WithMessage("Creation date is required");
        
        RuleFor(x => x.UpdatedAt)
            .NotEmpty().WithMessage("Update date is required");
        
        RuleFor(x => x.CancelledAt)
            .GreaterThan(x => x.CreatedAt).WithMessage("Cancellation date must be after creation date")
            .When(x => x.CancelledAt.HasValue);
        
        RuleFor(x => x.CancellationReason)
            .MaximumLength(500).WithMessage("Cancellation reason cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.CancellationReason));
    }
}
