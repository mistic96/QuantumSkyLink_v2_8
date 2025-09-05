using FluentValidation;
using MobileAPIGateway.Models.Cards;

namespace MobileAPIGateway.Validators.Cards;

/// <summary>
/// Validator for <see cref="DepositCardRequest"/>
/// </summary>
public class DepositCardRequestValidator : AbstractValidator<DepositCardRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DepositCardRequestValidator"/> class
    /// </summary>
    public DepositCardRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("Request ID is required");
            
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");
            
        RuleFor(x => x.ApplicationId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.ApplicationId))
            .WithMessage("Application ID cannot exceed 100 characters");
            
        RuleFor(x => x.UserId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.UserId))
            .WithMessage("User ID cannot exceed 100 characters");
            
        RuleFor(x => x.PaymentStatus)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.PaymentStatus))
            .WithMessage("Payment status cannot exceed 50 characters");
            
        RuleFor(x => x.PaymentId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.PaymentId))
            .WithMessage("Payment ID cannot exceed 100 characters");
            
        RuleFor(x => x.DepositId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.DepositId))
            .WithMessage("Deposit ID cannot exceed 100 characters");
            
        RuleFor(x => x.ReceiptNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.ReceiptNumber))
            .WithMessage("Receipt number cannot exceed 50 characters");
            
        RuleFor(x => x.ReceiptUrl)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.ReceiptUrl))
            .WithMessage("Receipt URL cannot exceed 255 characters");
            
        RuleForEach(x => x.Fees)
            .SetValidator(new ChargeRequestServiceFeeValidator())
            .When(x => x.Fees != null && x.Fees.Any());
            
        RuleFor(x => x.Error)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Error))
            .WithMessage("Error cannot exceed 255 characters");
            
        RuleFor(x => x.VendorChargeId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.VendorChargeId))
            .WithMessage("Vendor charge ID cannot exceed 100 characters");
            
        RuleFor(x => x.LocationId)
            .MaximumLength(32)
            .When(x => !string.IsNullOrEmpty(x.LocationId))
            .WithMessage("Location ID cannot exceed 32 characters");
    }
}
