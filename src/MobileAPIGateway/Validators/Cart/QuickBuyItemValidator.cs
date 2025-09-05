using FluentValidation;
using MobileAPIGateway.Models.Cart;

namespace MobileAPIGateway.Validators.Cart;

/// <summary>
/// Validator for <see cref="QuickBuyItem"/>
/// </summary>
public class QuickBuyItemValidator : AbstractValidator<QuickBuyItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuickBuyItemValidator"/> class
    /// </summary>
    public QuickBuyItemValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0");
            
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");
            
        RuleFor(x => x.TokenIndexId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.TokenIndexId))
            .WithMessage("Token index ID cannot exceed 100 characters");
            
        RuleFor(x => x.Name)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name cannot exceed 255 characters");
            
        RuleFor(x => x.Logo)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Logo))
            .WithMessage("Logo URL cannot exceed 1000 characters");
            
        RuleFor(x => x.Rate!)
            .SetValidator(new CloudExchangeRateValidator())
            .When(x => x.Rate != null);
            
        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total price must be greater than or equal to 0");
            
        RuleFor(x => x.MarketSymbol)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.MarketSymbol))
            .WithMessage("Market symbol cannot exceed 50 characters");
    }
}
