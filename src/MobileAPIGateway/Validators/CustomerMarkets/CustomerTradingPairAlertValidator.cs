using FluentValidation;
using MobileAPIGateway.Models.CustomerMarkets;

namespace MobileAPIGateway.Validators.CustomerMarkets;

/// <summary>
/// Validator for the customer trading pair alert model
/// </summary>
public class CustomerTradingPairAlertValidator : AbstractValidator<CustomerTradingPairAlert>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerTradingPairAlertValidator"/> class
    /// </summary>
    public CustomerTradingPairAlertValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Alert ID is required");
        
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
        
        RuleFor(x => x.TradingPairId)
            .NotEmpty().WithMessage("Trading pair ID is required");
        
        RuleFor(x => x.AlertType)
            .NotEmpty().WithMessage("Alert type is required")
            .MaximumLength(50).WithMessage("Alert type cannot exceed 50 characters");
        
        RuleFor(x => x.Condition)
            .NotEmpty().WithMessage("Alert condition is required")
            .MaximumLength(50).WithMessage("Alert condition cannot exceed 50 characters");
        
        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Alert value must be greater than 0");
        
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Alert status is required")
            .MaximumLength(50).WithMessage("Alert status cannot exceed 50 characters");
        
        RuleFor(x => x.NotificationMethod)
            .NotEmpty().WithMessage("Notification method is required")
            .MaximumLength(50).WithMessage("Notification method cannot exceed 50 characters");
        
        RuleFor(x => x.NotificationDestination)
            .NotEmpty().WithMessage("Notification destination is required")
            .MaximumLength(255).WithMessage("Notification destination cannot exceed 255 characters");
        
        RuleFor(x => x.CreatedAt)
            .NotEmpty().WithMessage("Creation date is required");
        
        RuleFor(x => x.UpdatedAt)
            .NotEmpty().WithMessage("Update date is required");
        
        RuleFor(x => x.LastTriggeredAt)
            .GreaterThan(x => x.CreatedAt).WithMessage("Last triggered date must be after creation date")
            .When(x => x.LastTriggeredAt.HasValue);
        
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(x => x.CreatedAt).WithMessage("Expiration date must be after creation date")
            .When(x => x.ExpiresAt.HasValue);
        
        RuleFor(x => x.RepeatIntervalMinutes)
            .GreaterThan(0).WithMessage("Repeat interval must be greater than 0")
            .When(x => x.IsRepeatable && x.RepeatIntervalMinutes.HasValue);
    }
}
