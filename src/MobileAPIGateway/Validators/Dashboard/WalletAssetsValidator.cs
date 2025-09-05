using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="WalletAssets"/>
/// </summary>
public class WalletAssetsValidator : AbstractValidator<WalletAssets>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WalletAssetsValidator"/> class
    /// </summary>
    public WalletAssetsValidator()
    {
        RuleForEach(x => x.Tokens)
            .SetValidator(new DynamicUserCoinValidator());
            
        RuleForEach(x => x.DigitalCurrencies)
            .SetValidator(new AssetBackedCurrencyValidator());
            
        RuleFor(x => x.TotalCoins)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total coins must be greater than or equal to 0");
            
        RuleFor(x => x.TotalDigitalCurrencies)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total digital currencies must be greater than or equal to 0");
            
        RuleFor(x => x.TotalCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total cost must be greater than or equal to 0");
            
        RuleFor(x => x.ProfitLossPercentage!)
            .SetValidator(new PercentageFluxValidator())
            .When(x => x.ProfitLossPercentage != null);
            
        RuleFor(x => x.TotalMarketPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total market price must be greater than or equal to 0");
            
        RuleForEach(x => x.Teams)
            .SetValidator(new PublicTeamExtendedPriceValidator());
    }
}
