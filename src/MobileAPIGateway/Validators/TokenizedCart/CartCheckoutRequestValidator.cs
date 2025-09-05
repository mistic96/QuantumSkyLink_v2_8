using FluentValidation;
using MobileAPIGateway.Models.TokenizedCart;

namespace MobileAPIGateway.Validators.TokenizedCart;

/// <summary>
/// Validator for the CartCheckoutRequest model
/// </summary>
public class CartCheckoutRequestValidator : AbstractValidator<CartCheckoutRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CartCheckoutRequestValidator"/> class
    /// </summary>
    public CartCheckoutRequestValidator()
    {
        RuleFor(x => x.PaymentMethodId)
            .NotEmpty().WithMessage("Payment method ID is required");
        
        RuleFor(x => x.BillingAddressId)
            .NotEmpty().WithMessage("Billing address ID is required");
        
        RuleFor(x => x.WalletAddress)
            .NotEmpty().WithMessage("Wallet address is required")
            .MinimumLength(26).WithMessage("Wallet address must be at least 26 characters")
            .MaximumLength(64).WithMessage("Wallet address cannot exceed 64 characters");
        
        RuleFor(x => x.PaymentCurrency)
            .NotEmpty().WithMessage("Payment currency is required")
            .Length(3).WithMessage("Currency code must be 3 characters");
        
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
    }
}
