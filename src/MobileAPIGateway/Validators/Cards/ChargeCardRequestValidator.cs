using FluentValidation;
using MobileAPIGateway.Models.Cards;

namespace MobileAPIGateway.Validators.Cards;

/// <summary>
/// Validator for <see cref="ChargeCardRequest"/>
/// </summary>
public class ChargeCardRequestValidator : AbstractValidator<ChargeCardRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChargeCardRequestValidator"/> class
    /// </summary>
    public ChargeCardRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("Request ID is required");
            
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("Application ID is required");
            
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
            
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");
            
        RuleFor(x => x.PaymentStatus)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.PaymentStatus))
            .WithMessage("Payment status cannot exceed 50 characters");
            
        RuleFor(x => x.PaymentId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.PaymentId))
            .WithMessage("Payment ID cannot exceed 100 characters");
            
        RuleFor(x => x.BlockChainDepositId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.BlockChainDepositId))
            .WithMessage("Blockchain deposit ID cannot exceed 100 characters");
            
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
            
        RuleForEach(x => x.TeamRevenues)
            .SetValidator(new TeamRevenueValidator())
            .When(x => x.TeamRevenues != null && x.TeamRevenues.Any());
            
        RuleFor(x => x.Error)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Error))
            .WithMessage("Error cannot exceed 255 characters");
            
        RuleFor(x => x.VendorChargeId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.VendorChargeId))
            .WithMessage("Vendor charge ID cannot exceed 100 characters");
            
        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithMessage("Location ID is required")
            .MaximumLength(32)
            .WithMessage("Location ID cannot exceed 32 characters");
            
        RuleFor(x => x.UserPaymentRequestUrl)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.UserPaymentRequestUrl))
            .WithMessage("User payment request URL cannot exceed 255 characters");
    }
}
