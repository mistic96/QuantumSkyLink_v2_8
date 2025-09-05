using FluentValidation;
using MobileAPIGateway.Models.Carts;

namespace MobileAPIGateway.Validators.Carts;

/// <summary>
/// Validator for the CartResponse model
/// </summary>
public class CartResponseValidator : AbstractValidator<CartResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CartResponseValidator"/> class
    /// </summary>
    public CartResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required");
        
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required");
        
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Cart ID is required");
        
        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required");
        
        When(x => x.Cart != null, () =>
        {
            RuleFor(x => x.Cart)
                .SetValidator(new ShoppingCartValidator());
        });
    }
}
