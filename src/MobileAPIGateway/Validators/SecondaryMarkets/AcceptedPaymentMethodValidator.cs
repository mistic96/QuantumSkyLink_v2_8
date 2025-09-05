using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="AcceptedPaymentMethod"/>
/// </summary>
public class AcceptedPaymentMethodValidator : AbstractValidator<AcceptedPaymentMethod>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AcceptedPaymentMethodValidator"/> class
    /// </summary>
    public AcceptedPaymentMethodValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
    }
}
