using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="PaymentInfo"/>
/// </summary>
public class PaymentInfoValidator : AbstractValidator<PaymentInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentInfoValidator"/> class
    /// </summary>
    public PaymentInfoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.WalletPaymentId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.WalletPaymentId))
            .WithMessage("Wallet payment ID cannot exceed 100 characters");
            
        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total amount must be greater than or equal to 0");
    }
}
