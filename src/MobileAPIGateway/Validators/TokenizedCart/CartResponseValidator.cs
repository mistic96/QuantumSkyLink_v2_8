using FluentValidation;
using MobileAPIGateway.Models.TokenizedCart;

namespace MobileAPIGateway.Validators.TokenizedCart;

/// <summary>
/// Validator for the CartResponse model
/// </summary>
public class CartResponseValidator : AbstractValidator<CartResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CartResponseValidator"/> class
    /// </summary>
    public CartResponseValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Response ID is required");
        
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required");
        
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required");
        
        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required");
        
        When(x => x.Cart != null, () =>
        {
            RuleFor(x => x.Cart)
                .SetValidator(new TokenizedCartValidator());
        });
    }
}
