using FluentValidation;
using MobileAPIGateway.Models.Cart;

namespace MobileAPIGateway.Validators.Cart;

/// <summary>
/// Validator for <see cref="CloudCart"/>
/// </summary>
public class CloudCartValidator : AbstractValidator<CloudCart>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudCartValidator"/> class
    /// </summary>
    public CloudCartValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty()
            .WithMessage("Cart ID is required");
            
        RuleFor(x => x.CartOwnerId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.CartOwnerId))
            .WithMessage("Cart owner ID cannot exceed 100 characters");
            
        RuleForEach(x => x.Items)
            .SetValidator(new CartItemValidator())
            .When(x => x.Items != null && x.Items.Any());
            
        RuleFor(x => x.AdHocItem!)
            .SetValidator(new QuickBuyItemValidator())
            .When(x => x.AdHocItem != null);
            
        RuleFor(x => x.ItemDiscount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ItemDiscount.HasValue)
            .WithMessage("Item discount must be greater than or equal to 0");
            
        RuleFor(x => x.DiscountCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.DiscountCode))
            .WithMessage("Discount code cannot exceed 50 characters");
            
        RuleFor(x => x.MinimumCartAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum cart amount must be greater than or equal to 0");
            
        RuleFor(x => x.TotalDiscount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total discount must be greater than or equal to 0");
            
        RuleFor(x => x.NumberOfItems)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Number of items must be greater than or equal to 0");
            
        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total price must be greater than or equal to 0");
            
        RuleForEach(x => x.Messages)
            .SetValidator(new CartMessagesValidator())
            .When(x => x.Messages != null && x.Messages.Any());
            
        RuleFor(x => x.Summary!)
            .SetValidator(new CloudCartSummarySectionValidator())
            .When(x => x.Summary != null);
    }
}
