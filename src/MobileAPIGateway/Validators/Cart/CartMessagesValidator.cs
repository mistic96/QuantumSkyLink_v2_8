using FluentValidation;
using MobileAPIGateway.Models.Cart;

namespace MobileAPIGateway.Validators.Cart;

/// <summary>
/// Validator for <see cref="CartMessages"/>
/// </summary>
public class CartMessagesValidator : AbstractValidator<CartMessages>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CartMessagesValidator"/> class
    /// </summary>
    public CartMessagesValidator()
    {
        RuleFor(x => x.Message)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Message))
            .WithMessage("Message cannot exceed 500 characters");
    }
}
