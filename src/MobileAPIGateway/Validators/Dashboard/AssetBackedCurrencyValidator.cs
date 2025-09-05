using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="AssetBackedCurrency"/>
/// </summary>
public class AssetBackedCurrencyValidator : AbstractValidator<AssetBackedCurrency>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssetBackedCurrencyValidator"/> class
    /// </summary>
    public AssetBackedCurrencyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");
            
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be greater than or equal to 0");
    }
}
