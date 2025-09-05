using FluentValidation;
using MobileAPIGateway.Models.Wallet;

namespace MobileAPIGateway.Validators.Wallet;

/// <summary>
/// Validator for the wallet transaction model
/// </summary>
public class WalletTransactionValidator : AbstractValidator<WalletTransaction>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WalletTransactionValidator"/> class
    /// </summary>
    public WalletTransactionValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Transaction ID is required");
        
        RuleFor(x => x.WalletId)
            .NotEmpty().WithMessage("Wallet ID is required");
        
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
        
        RuleFor(x => x.TransactionType)
            .NotEmpty().WithMessage("Transaction type is required");
        
        RuleFor(x => x.Amount)
            .NotEqual(0).WithMessage("Amount cannot be 0");
        
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3, 10).WithMessage("Currency code must be between 3 and 10 characters");
        
        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required");
        
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required");
        
        RuleFor(x => x.ReferenceId)
            .NotEmpty().WithMessage("Reference ID is required");
        
        RuleFor(x => x.FeeCurrencyCode)
            .NotEmpty().WithMessage("Fee currency code is required")
            .Length(3, 10).WithMessage("Fee currency code must be between 3 and 10 characters")
            .When(x => x.FeeAmount > 0);
        
        RuleFor(x => x.BlockchainTransactionId)
            .NotEmpty().WithMessage("Blockchain transaction ID is required")
            .When(x => !string.IsNullOrEmpty(x.BlockchainNetwork));
        
        RuleFor(x => x.BlockchainNetwork)
            .NotEmpty().WithMessage("Blockchain network is required")
            .When(x => !string.IsNullOrEmpty(x.BlockchainTransactionId));
    }
}
