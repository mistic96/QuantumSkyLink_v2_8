using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="MicroServiceChargeRequestFee"/>
/// </summary>
public class MicroServiceChargeRequestFeeValidator : AbstractValidator<MicroServiceChargeRequestFee>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MicroServiceChargeRequestFeeValidator"/> class
    /// </summary>
    public MicroServiceChargeRequestFeeValidator()
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
