using FluentValidation;
using MobileAPIGateway.Models.TokenizedCart;

namespace MobileAPIGateway.Validators.TokenizedCart;

/// <summary>
/// Validator for the CartCreationRequest model
/// </summary>
public class CartCreationRequestValidator : AbstractValidator<CartCreationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CartCreationRequestValidator"/> class
    /// </summary>
    public CartCreationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cart name is required")
            .MaximumLength(100).WithMessage("Cart name cannot exceed 100 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Cart description cannot exceed 500 characters");
        
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency code must be 3 characters");
        
        RuleForEach(x => x.Items)
            .SetValidator(new CartItemRequestValidator());
    }
}

/// <summary>
/// Validator for the CartItemRequest model
/// </summary>
public class CartItemRequestValidator : AbstractValidator<CartItemRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CartItemRequestValidator"/> class
    /// </summary>
    public CartItemRequestValidator()
    {
        RuleFor(x => x.TokenId)
            .NotEmpty().WithMessage("Token ID is required");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
    }
}
