using FluentValidation;
using MobileAPIGateway.Models.TokenizedCart;

namespace MobileAPIGateway.Validators.TokenizedCart;

/// <summary>
/// Validator for the TokenizedCart model
/// </summary>
public class TokenizedCartValidator : AbstractValidator<Models.TokenizedCart.TokenizedCart>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenizedCartValidator"/> class
    /// </summary>
    public TokenizedCartValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Cart ID is required");
        
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cart name is required")
            .MaximumLength(100).WithMessage("Cart name cannot exceed 100 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Cart description cannot exceed 500 characters");
        
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Cart status is required");
        
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency code must be 3 characters");
        
        RuleForEach(x => x.Items)
            .SetValidator(new TokenizedCartItemValidator());
    }
}
