using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="OrderPaymentAddress"/>
/// </summary>
public class OrderPaymentAddressValidator : AbstractValidator<OrderPaymentAddress>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderPaymentAddressValidator"/> class
    /// </summary>
    public OrderPaymentAddressValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.StreetAddress1)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.StreetAddress1))
            .WithMessage("Street address 1 cannot exceed 100 characters");
            
        RuleFor(x => x.StreetAddress2)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.StreetAddress2))
            .WithMessage("Street address 2 cannot exceed 100 characters");
            
        RuleFor(x => x.City)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.City))
            .WithMessage("City cannot exceed 50 characters");
            
        RuleFor(x => x.State)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.State))
            .WithMessage("State cannot exceed 50 characters");
            
        RuleFor(x => x.PostalCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.PostalCode))
            .WithMessage("Postal code cannot exceed 20 characters");
            
        RuleFor(x => x.Country)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Country))
            .WithMessage("Country cannot exceed 50 characters");
            
        RuleFor(x => x.CountryAlpha2Code)
            .MaximumLength(2)
            .When(x => !string.IsNullOrEmpty(x.CountryAlpha2Code))
            .WithMessage("Country alpha-2 code must be 2 characters");
    }
}
