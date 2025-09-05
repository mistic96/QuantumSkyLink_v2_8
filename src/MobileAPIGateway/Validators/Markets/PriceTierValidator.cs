using FluentValidation;
using MobileAPIGateway.Models.Markets;

namespace MobileAPIGateway.Validators.Markets;

/// <summary>
/// Validator for the price tier model
/// </summary>
public class PriceTierValidator : AbstractValidator<PriceTier>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PriceTierValidator"/> class
    /// </summary>
    public PriceTierValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Price tier ID is required");
        
        RuleFor(x => x.MinQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum quantity must be greater than or equal to 0");
        
        RuleFor(x => x.MaxQuantity)
            .GreaterThan(x => x.MinQuantity).WithMessage("Maximum quantity must be greater than minimum quantity")
            .When(x => x.MaxQuantity.HasValue);
        
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");
        
        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100")
            .When(x => x.DiscountPercentage.HasValue);
        
        RuleFor(x => x.Name)
            .MaximumLength(50).WithMessage("Name cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));
        
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
