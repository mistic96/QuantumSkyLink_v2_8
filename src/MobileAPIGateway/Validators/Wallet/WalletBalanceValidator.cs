using FluentValidation;
using MobileAPIGateway.Models.Wallet;

namespace MobileAPIGateway.Validators.Wallet;

/// <summary>
/// Validator for the wallet balance model
/// </summary>
public class WalletBalanceValidator : AbstractValidator<WalletBalance>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WalletBalanceValidator"/> class
    /// </summary>
    public WalletBalanceValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithMessage("Wallet ID is required");
        
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
        
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3, 10).WithMessage("Currency code must be between 3 and 10 characters");
        
        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Balance must be greater than or equal to 0");
        
        RuleFor(x => x.AvailableBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Available balance must be greater than or equal to 0");
        
        RuleFor(x => x.PendingBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Pending balance must be greater than or equal to 0");
        
        RuleFor(x => x.LockedBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Locked balance must be greater than or equal to 0");
        
        RuleFor(x => x.LastUpdated)
            .NotEmpty().WithMessage("Last updated date is required");
    }
}
