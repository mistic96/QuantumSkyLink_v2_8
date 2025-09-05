using FluentValidation;
using MobileAPIGateway.Models.Carts;

namespace MobileAPIGateway.Validators.Carts;

/// <summary>
/// Validator for the CheckoutCartRequest model
/// </summary>
public class CheckoutCartRequestValidator : AbstractValidator<CheckoutCartRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CheckoutCartRequestValidator"/> class
    /// </summary>
    public CheckoutCartRequestValidator()
    {
        RuleFor(x => x.ShippingAddressId)
            .NotEmpty().WithMessage("Shipping address ID is required");
        
        RuleFor(x => x.BillingAddressId)
            .NotEmpty().WithMessage("Billing address ID is required");
        
        RuleFor(x => x.PaymentMethodId)
            .NotEmpty().WithMessage("Payment method ID is required");
        
        RuleFor(x => x.ShippingMethod)
            .NotEmpty().WithMessage("Shipping method is required");
    }
}
