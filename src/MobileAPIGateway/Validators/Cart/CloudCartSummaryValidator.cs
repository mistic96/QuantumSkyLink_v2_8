using FluentValidation;
using MobileAPIGateway.Models.Cart;

namespace MobileAPIGateway.Validators.Cart;

/// <summary>
/// Validator for <see cref="CloudCartSummary"/>
/// </summary>
public class CloudCartSummaryValidator : AbstractValidator<CloudCartSummary>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudCartSummaryValidator"/> class
    /// </summary>
    public CloudCartSummaryValidator()
    {
        RuleFor(x => x.UntilPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Unit price must be greater than or equal to 0");
            
        RuleFor(x => x.TotalItems)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total items must be greater than or equal to 0");
            
        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount must be greater than or equal to 0");
            
        RuleFor(x => x.TotalDiscount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total discount must be greater than or equal to 0");
            
        RuleFor(x => x.Total)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total must be greater than or equal to 0");
    }
}
