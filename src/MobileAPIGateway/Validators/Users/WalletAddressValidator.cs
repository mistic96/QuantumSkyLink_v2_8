using FluentValidation;
using MobileAPIGateway.Models.Users;

namespace MobileAPIGateway.Validators.Users;

/// <summary>
/// Validator for <see cref="WalletAddress"/>
/// </summary>
public class WalletAddressValidator : AbstractValidator<WalletAddress>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WalletAddressValidator"/> class
    /// </summary>
    public WalletAddressValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty().When(x => !string.IsNullOrEmpty(x.OwnerId))
            .WithMessage("Owner ID is required");
            
        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name cannot exceed 100 characters");
            
        RuleFor(x => x.CryptoPayoutAddress)
            .NotEmpty().When(x => !string.IsNullOrEmpty(x.CryptoPayoutAddress))
            .WithMessage("Crypto payout address is required")
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.CryptoPayoutAddress))
            .WithMessage("Crypto payout address cannot exceed 255 characters");
            
        RuleFor(x => x.Logo)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Logo))
            .WithMessage("Logo URL cannot exceed 255 characters");
    }
}
