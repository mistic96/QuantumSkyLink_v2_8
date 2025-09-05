using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="UserCoinMetric"/>
/// </summary>
public class UserCoinMetricValidator : AbstractValidator<UserCoinMetric>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserCoinMetricValidator"/> class
    /// </summary>
    public UserCoinMetricValidator()
    {
        RuleFor(x => x.UserWallet!)
            .SetValidator(new WalletExtendedValidator())
            .When(x => x.UserWallet != null);
            
        RuleFor(x => x.UserWalletTotalCoins)
            .GreaterThanOrEqualTo(0)
            .WithMessage("User wallet total coins must be greater than or equal to 0");
            
        RuleFor(x => x.OnHoldTotalCoins)
            .GreaterThanOrEqualTo(0)
            .WithMessage("On hold total coins must be greater than or equal to 0");
            
        RuleFor(x => x.UserWalletDigitalCurrencies)
            .GreaterThanOrEqualTo(0)
            .WithMessage("User wallet digital currencies must be greater than or equal to 0");
            
        RuleFor(x => x.OnHoldTotalDigitalCurrencies)
            .GreaterThanOrEqualTo(0)
            .WithMessage("On hold total digital currencies must be greater than or equal to 0");
            
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
