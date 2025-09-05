using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="CloudFee"/>
/// </summary>
public class CloudFeeValidator : AbstractValidator<CloudFee>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudFeeValidator"/> class
    /// </summary>
    public CloudFeeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be greater than or equal to 0");
            
        RuleFor(x => x.Description)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 255 characters");
    }
}
