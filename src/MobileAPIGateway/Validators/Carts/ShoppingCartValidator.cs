using FluentValidation;
using MobileAPIGateway.Models.Carts;

namespace MobileAPIGateway.Validators.Carts;

/// <summary>
/// Validator for the ShoppingCart model
/// </summary>
public class ShoppingCartValidator : AbstractValidator<ShoppingCart>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShoppingCartValidator"/> class
    /// </summary>
    public ShoppingCartValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Cart ID is required");
        
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
        
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required");
        
        RuleFor(x => x.CreatedAt)
            .NotEmpty().WithMessage("Created date is required");
        
        RuleFor(x => x.UpdatedAt)
            .NotEmpty().WithMessage("Updated date is required");
        
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items cannot be null");
        
        RuleForEach(x => x.Items)
            .SetValidator(new CartItemValidator());
        
        RuleFor(x => x.Subtotal)
            .GreaterThanOrEqualTo(0).WithMessage("Subtotal must be greater than or equal to 0");
        
        RuleFor(x => x.Tax)
            .GreaterThanOrEqualTo(0).WithMessage("Tax must be greater than or equal to 0");
        
        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("Shipping cost must be greater than or equal to 0");
        
        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount must be greater than or equal to 0");
        
        RuleFor(x => x.Total)
            .GreaterThanOrEqualTo(0).WithMessage("Total must be greater than or equal to 0");
        
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3).WithMessage("Currency code must be 3 characters");
    }
}
