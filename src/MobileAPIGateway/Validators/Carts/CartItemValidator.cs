using FluentValidation;
using MobileAPIGateway.Models.Carts;

namespace MobileAPIGateway.Validators.Carts;

/// <summary>
/// Validator for the CartItem model
/// </summary>
public class CartItemValidator : AbstractValidator<CartItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CartItemValidator"/> class
    /// </summary>
    public CartItemValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Item ID is required");
        
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Cart ID is required");
        
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
        
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
        
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");
        
        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount must be greater than or equal to 0");
        
        RuleFor(x => x.Tax)
            .GreaterThanOrEqualTo(0).WithMessage("Tax must be greater than or equal to 0");
        
        RuleFor(x => x.Subtotal)
            .GreaterThanOrEqualTo(0).WithMessage("Subtotal must be greater than or equal to 0");
        
        RuleFor(x => x.Total)
            .GreaterThanOrEqualTo(0).WithMessage("Total must be greater than or equal to 0");
        
        RuleFor(x => x.CreatedAt)
            .NotEmpty().WithMessage("Created date is required");
        
        RuleFor(x => x.UpdatedAt)
            .NotEmpty().WithMessage("Updated date is required");
    }
}
