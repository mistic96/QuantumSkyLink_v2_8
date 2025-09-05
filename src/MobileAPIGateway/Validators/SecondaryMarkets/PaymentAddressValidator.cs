using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="PaymentAddress"/>
/// </summary>
public class PaymentAddressValidator : AbstractValidator<PaymentAddress>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentAddressValidator"/> class
    /// </summary>
    public PaymentAddressValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.Address)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Address))
            .WithMessage("Address cannot exceed 255 characters");
            
        RuleFor(x => x.ExpectedCryptoAmount)
            .GreaterThan(0)
            .WithMessage("Expected crypto amount must be greater than 0");
            
        RuleFor(x => x.Rate)
            .GreaterThan(0)
            .WithMessage("Rate must be greater than 0");
            
        RuleFor(x => x.AddressAsQrCode)
            .MaximumLength(10000)
            .When(x => !string.IsNullOrEmpty(x.AddressAsQrCode))
            .WithMessage("Address as QR code cannot exceed 10000 characters");
    }
}
