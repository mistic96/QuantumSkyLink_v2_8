using FluentValidation;
using MobileAPIGateway.Models.Carts;

namespace MobileAPIGateway.Validators.Carts;

/// <summary>
/// Validator for the CreateCartRequest model
/// </summary>
public class CreateCartRequestValidator : AbstractValidator<CreateCartRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCartRequestValidator"/> class
    /// </summary>
    public CreateCartRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items cannot be null");
        
        RuleForEach(x => x.Items)
            .SetValidator(new CreateCartItemRequestValidator());
        
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3).WithMessage("Currency code must be 3 characters");
    }
}

/// <summary>
/// Validator for the CreateCartItemRequest model
/// </summary>
public class CreateCartItemRequestValidator : AbstractValidator<CreateCartItemRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCartItemRequestValidator"/> class
    /// </summary>
    public CreateCartItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
    }
}
