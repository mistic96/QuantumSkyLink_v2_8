using FluentValidation;
using MobileAPIGateway.Models.CardManagement;

namespace MobileAPIGateway.Validators.CardManagement;

/// <summary>
/// Validator for the PaymentCard model
/// </summary>
public class PaymentCardValidator : AbstractValidator<PaymentCard>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentCardValidator"/> class
    /// </summary>
    public PaymentCardValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Card ID is required");
        
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
        
        RuleFor(x => x.CardType)
            .NotEmpty().WithMessage("Card type is required");
        
        RuleFor(x => x.MaskedNumber)
            .NotEmpty().WithMessage("Masked number is required")
            .Matches(@"^\*+\d+$").WithMessage("Masked number must be in the format '**** **** **** 1234'");
        
        RuleFor(x => x.CardholderName)
            .NotEmpty().WithMessage("Cardholder name is required")
            .MaximumLength(100).WithMessage("Cardholder name cannot exceed 100 characters");
        
        RuleFor(x => x.ExpirationMonth)
            .InclusiveBetween(1, 12).WithMessage("Expiration month must be between 1 and 12");
        
        RuleFor(x => x.ExpirationYear)
            .GreaterThanOrEqualTo(DateTime.Now.Year).WithMessage("Expiration year must be in the future");
        
        RuleFor(x => x.BillingAddressId)
            .NotEmpty().WithMessage("Billing address ID is required");
        
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required");
        
        RuleFor(x => x.ProcessorToken)
            .NotEmpty().WithMessage("Processor token is required");
    }
}
