using FluentValidation;
using MobileAPIGateway.Models.Wallet;

namespace MobileAPIGateway.Validators.Wallet;

/// <summary>
/// Validator for the deposit request model
/// </summary>
public class DepositRequestValidator : AbstractValidator<DepositRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DepositRequestValidator"/> class
    /// </summary>
    public DepositRequestValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithMessage("Wallet ID is required");
        
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3, 10).WithMessage("Currency code must be between 3 and 10 characters");
        
        RuleFor(x => x.BlockchainNetwork)
            .NotEmpty().WithMessage("Blockchain network is required");
        
        RuleFor(x => x.ReferenceId)
            .MaximumLength(100).WithMessage("Reference ID cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceId));
    }
}
