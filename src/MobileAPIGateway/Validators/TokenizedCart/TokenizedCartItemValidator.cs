using FluentValidation;
using MobileAPIGateway.Models.TokenizedCart;

namespace MobileAPIGateway.Validators.TokenizedCart;

/// <summary>
/// Validator for the TokenizedCartItem model
/// </summary>
public class TokenizedCartItemValidator : AbstractValidator<TokenizedCartItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenizedCartItemValidator"/> class
    /// </summary>
    public TokenizedCartItemValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Item ID is required");
        
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Cart ID is required");
        
        RuleFor(x => x.TokenId)
            .NotEmpty().WithMessage("Token ID is required");
        
        RuleFor(x => x.TokenName)
            .NotEmpty().WithMessage("Token name is required")
            .MaximumLength(100).WithMessage("Token name cannot exceed 100 characters");
        
        RuleFor(x => x.TokenSymbol)
            .NotEmpty().WithMessage("Token symbol is required")
            .MaximumLength(10).WithMessage("Token symbol cannot exceed 10 characters");
        
        RuleFor(x => x.AssetType)
            .NotEmpty().WithMessage("Asset type is required");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
        
        RuleFor(x => x.PricePerUnit)
            .GreaterThan(0).WithMessage("Price per unit must be greater than 0");
        
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency code must be 3 characters");
        
        RuleFor(x => x.TotalValue)
            .GreaterThan(0).WithMessage("Total value must be greater than 0");
    }
}
