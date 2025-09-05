using FluentValidation;
using MobileAPIGateway.Models.Wallet;

namespace MobileAPIGateway.Validators.Wallet;

/// <summary>
/// Validator for the withdraw request model
/// </summary>
public class WithdrawRequestValidator : AbstractValidator<WithdrawRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WithdrawRequestValidator"/> class
    /// </summary>
    public WithdrawRequestValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithMessage("Wallet ID is required");
        
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount is required")
            .GreaterThan(0).WithMessage("Amount must be greater than 0");
        
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3, 10).WithMessage("Currency code must be between 3 and 10 characters");
        
        RuleFor(x => x.DestinationAddress)
            .NotEmpty().WithMessage("Destination address is required");
        
        RuleFor(x => x.BlockchainNetwork)
            .NotEmpty().WithMessage("Blockchain network is required");
        
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
        
        RuleFor(x => x.ReferenceId)
            .MaximumLength(100).WithMessage("Reference ID cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceId));
        
        RuleFor(x => x.Memo)
            .MaximumLength(100).WithMessage("Memo cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Memo));
    }
}
