using FluentValidation;
using MobileAPIGateway.Models.TokenizedCart;

namespace MobileAPIGateway.Validators.TokenizedCart;

/// <summary>
/// Validator for the CartUpdateRequest model
/// </summary>
public class CartUpdateRequestValidator : AbstractValidator<CartUpdateRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CartUpdateRequestValidator"/> class
    /// </summary>
    public CartUpdateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cart name is required")
            .MaximumLength(100).WithMessage("Cart name cannot exceed 100 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Cart description cannot exceed 500 characters");
        
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency code must be 3 characters");
    }
}
