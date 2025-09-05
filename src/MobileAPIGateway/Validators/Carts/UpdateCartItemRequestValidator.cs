using FluentValidation;
using MobileAPIGateway.Models.Carts;

namespace MobileAPIGateway.Validators.Carts;

/// <summary>
/// Validator for the UpdateCartItemRequest model
/// </summary>
public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCartItemRequestValidator"/> class
    /// </summary>
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
    }
}
