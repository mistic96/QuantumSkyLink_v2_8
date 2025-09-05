using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="MicroDepositCryptoResponse"/>
/// </summary>
public class MicroDepositCryptoResponseValidator : AbstractValidator<MicroDepositCryptoResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MicroDepositCryptoResponseValidator"/> class
    /// </summary>
    public MicroDepositCryptoResponseValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("Request ID is required");
            
        RuleFor(x => x.AmountInUsd)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount in USD must be greater than or equal to 0");
            
        RuleFor(x => x.TransactionSetId)
            .NotEmpty()
            .WithMessage("Transaction set ID is required");
            
        RuleFor(x => x.PaymentAddress!)
            .SetValidator(new PaymentAddressValidator())
            .When(x => x.PaymentAddress != null);
            
        RuleFor(x => x.FeesTotalInUsd)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Fees total in USD must be greater than or equal to 0");
            
        RuleFor(x => x.TotalChargeAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total charge amount must be greater than or equal to 0");
            
        RuleFor(x => x.VendorPaymentCode)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.VendorPaymentCode))
            .WithMessage("Vendor payment code cannot exceed 100 characters");
            
        RuleFor(x => x.InteractivePaymentFlowUrl)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.InteractivePaymentFlowUrl))
            .WithMessage("Interactive payment flow URL cannot exceed 2000 characters");
            
        RuleFor(x => x.VendorChargeId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.VendorChargeId))
            .WithMessage("Vendor charge ID cannot exceed 100 characters");
            
        RuleFor(x => x.Error)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Error))
            .WithMessage("Error cannot exceed 1000 characters");
            
        RuleForEach(x => x.Fees)
            .SetValidator(new MicroServiceChargeRequestFeeValidator());
    }
}
