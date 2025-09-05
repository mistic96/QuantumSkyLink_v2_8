using FluentValidation;
using MobileAPIGateway.Models.Carts;

namespace MobileAPIGateway.Validators.Carts;

/// <summary>
/// Validator for the UpdateCartRequest model
/// </summary>
public class UpdateCartRequestValidator : AbstractValidator<UpdateCartRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCartRequestValidator"/> class
    /// </summary>
    public UpdateCartRequestValidator()
    {
        RuleFor(x => x.CurrencyCode)
            .Length(3).WithMessage("Currency code must be 3 characters")
            .When(x => !string.IsNullOrEmpty(x.CurrencyCode));
    }
}
