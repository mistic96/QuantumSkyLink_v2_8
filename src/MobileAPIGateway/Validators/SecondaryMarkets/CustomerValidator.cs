using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="Customer"/>
/// </summary>
public class CustomerValidator : AbstractValidator<Customer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerValidator"/> class
    /// </summary>
    public CustomerValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email must be a valid email address");
            
        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone cannot exceed 20 characters");
            
        RuleFor(x => x.Address!)
            .SetValidator(new OrderPaymentAddressValidator())
            .When(x => x.Address != null);
            
        RuleFor(x => x.PaymentInfo!)
            .SetValidator(new PaymentInfoValidator())
            .When(x => x.PaymentInfo != null);
            
        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.FirstName))
            .WithMessage("First name cannot exceed 50 characters");
            
        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.LastName))
            .WithMessage("Last name cannot exceed 50 characters");
            
        RuleFor(x => x.PreferredName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.PreferredName))
            .WithMessage("Preferred name cannot exceed 50 characters");
            
        RuleFor(x => x.ExternalId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.ExternalId))
            .WithMessage("External ID cannot exceed 100 characters");
            
        RuleFor(x => x.IdentityTotalTransactionAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Identity total transaction amount must be greater than or equal to 0");
    }
}
