using FluentValidation;
using MobileAPIGateway.Models.Cart;

namespace MobileAPIGateway.Validators.Cart;

/// <summary>
/// Validator for <see cref="CloudCartSummarySection"/>
/// </summary>
public class CloudCartSummarySectionValidator : AbstractValidator<CloudCartSummarySection>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudCartSummarySectionValidator"/> class
    /// </summary>
    public CloudCartSummarySectionValidator()
    {
        RuleForEach(x => x.CurrentExchangeRates)
            .SetValidator(new ExchangeRateValidator())
            .When(x => x.CurrentExchangeRates != null && x.CurrentExchangeRates.Any());
            
        RuleForEach(x => x.Fees)
            .SetValidator(new CloudFeeValidator())
            .When(x => x.Fees != null && x.Fees.Any());
            
        RuleFor(x => x.PrimaryMarket!)
            .SetValidator(new CloudCartSummaryValidator())
            .When(x => x.PrimaryMarket != null);
            
        RuleFor(x => x.SecondaryMarket!)
            .SetValidator(new CloudCartSummaryValidator())
            .When(x => x.SecondaryMarket != null);
            
        RuleFor(x => x.TotalDiscount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total discount must be greater than or equal to 0");
            
        RuleFor(x => x.NumberOfItems)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Number of items must be greater than or equal to 0");
            
        RuleFor(x => x.TotalFees)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total fees must be greater than or equal to 0");
            
        RuleFor(x => x.MarketsTotal)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Markets total must be greater than or equal to 0");
    }
}
