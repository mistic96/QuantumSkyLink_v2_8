using FluentValidation;
using MobileAPIGateway.Models.Cards;

namespace MobileAPIGateway.Validators.Cards;

/// <summary>
/// Validator for <see cref="ChargeRequestServiceFee"/>
/// </summary>
public class ChargeRequestServiceFeeValidator : AbstractValidator<ChargeRequestServiceFee>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChargeRequestServiceFeeValidator"/> class
    /// </summary>
    public ChargeRequestServiceFeeValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be greater than or equal to 0");
            
        RuleFor(x => x.Description)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 255 characters");
            
        RuleFor(x => x.TeamId)
            .GreaterThan(0)
            .WithMessage("Team ID must be greater than 0");
    }
}
