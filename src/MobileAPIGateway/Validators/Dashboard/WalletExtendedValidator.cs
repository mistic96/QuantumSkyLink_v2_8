using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="WalletExtended"/>
/// </summary>
public class WalletExtendedValidator : AbstractValidator<WalletExtended>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WalletExtendedValidator"/> class
    /// </summary>
    public WalletExtendedValidator()
    {
        RuleFor(x => x.InWallet!)
            .SetValidator(new WalletAssetsValidator())
            .When(x => x.InWallet != null);
            
        RuleFor(x => x.OnHold!)
            .SetValidator(new WalletAssetsValidator())
            .When(x => x.OnHold != null);
    }
}
