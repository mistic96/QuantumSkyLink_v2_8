using System;
using FluentValidation;
using MobileAPIGateway.Models.CardManagement;

namespace MobileAPIGateway.Validators.CardManagement;

/// <summary>
/// Validator for the AddCardRequest model
/// </summary>
public class AddCardRequestValidator : AbstractValidator<AddCardRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddCardRequestValidator"/> class
    /// </summary>
    public AddCardRequestValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required")
            .CreditCard().WithMessage("Card number is not valid");
        
        RuleFor(x => x.CardholderName)
            .NotEmpty().WithMessage("Cardholder name is required")
            .MaximumLength(100).WithMessage("Cardholder name cannot exceed 100 characters");
        
        RuleFor(x => x.ExpirationMonth)
            .NotEmpty().WithMessage("Expiration month is required")
            .InclusiveBetween(1, 12).WithMessage("Expiration month must be between 1 and 12");
        
        RuleFor(x => x.ExpirationYear)
            .NotEmpty().WithMessage("Expiration year is required")
            .GreaterThanOrEqualTo(DateTime.Now.Year).WithMessage("Expiration year must be in the future");
        
        RuleFor(x => x)
            .Must(x => new DateTime(x.ExpirationYear, x.ExpirationMonth, 1).AddMonths(1).AddDays(-1) >= DateTime.Now)
            .WithMessage("Card has expired");
        
        RuleFor(x => x.Cvv)
            .NotEmpty().WithMessage("CVV is required")
            .Matches(@"^\d{3,4}$").WithMessage("CVV must be 3 or 4 digits");
        
        RuleFor(x => x.BillingAddressId)
            .NotEmpty().WithMessage("Billing address ID is required");
    }
}
