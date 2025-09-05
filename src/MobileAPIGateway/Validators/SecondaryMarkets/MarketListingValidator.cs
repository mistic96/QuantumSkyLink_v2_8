using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="MarketListing"/>
/// </summary>
public class MarketListingValidator : AbstractValidator<MarketListing>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarketListingValidator"/> class
    /// </summary>
    public MarketListingValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.SellerId)
            .NotEmpty()
            .WithMessage("Seller ID is required");
            
        RuleFor(x => x.MarketSymbol)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.MarketSymbol))
            .WithMessage("Market symbol cannot exceed 50 characters");
            
        RuleFor(x => x.MainImage!)
            .SetValidator(new ImageValidator())
            .When(x => x.MainImage != null);
            
        RuleForEach(x => x.ImageGallery)
            .SetValidator(new ImageValidator())
            .When(x => x.ImageGallery != null && x.ImageGallery.Any());
            
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .When(x => x.Quantity.HasValue)
            .WithMessage("Quantity must be greater than 0");
            
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.Price.HasValue)
            .WithMessage("Price must be greater than 0");
            
        RuleFor(x => x.StartingPrice)
            .GreaterThan(0)
            .When(x => x.StartingPrice.HasValue)
            .WithMessage("Starting price must be greater than 0");
            
        RuleFor(x => x.MaximumPrice)
            .GreaterThan(0)
            .When(x => x.MaximumPrice.HasValue)
            .WithMessage("Maximum price must be greater than 0");
            
        RuleFor(x => x.MinimumPrice)
            .GreaterThan(0)
            .When(x => x.MinimumPrice.HasValue)
            .WithMessage("Minimum price must be greater than 0");
            
        RuleFor(x => x.Margin)
            .GreaterThan(0)
            .When(x => x.Margin.HasValue)
            .WithMessage("Margin must be greater than 0");
            
        RuleFor(x => x.Unit)
            .GreaterThan(0)
            .When(x => x.Unit.HasValue)
            .WithMessage("Unit must be greater than 0");
            
        RuleFor(x => x.CurrentRate!)
            .SetValidator(new LedgerMarketExchangeRateValidator())
            .When(x => x.CurrentRate != null);
            
        RuleForEach(x => x.AcceptedPaymentMethods)
            .SetValidator(new AcceptedPaymentMethodValidator())
            .When(x => x.AcceptedPaymentMethods != null && x.AcceptedPaymentMethods.Any());
            
        RuleFor(x => x.Message)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Message))
            .WithMessage("Message cannot exceed 1000 characters");
            
        RuleForEach(x => x.Fees)
            .SetValidator(new CloudFeeValidator())
            .When(x => x.Fees != null && x.Fees.Any());
            
        RuleFor(x => x.TotalPriceInUsd)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total price in USD must be greater than or equal to 0");
            
        RuleFor(x => x.Customer!)
            .SetValidator(new CustomerValidator())
            .When(x => x.Customer != null);
            
        RuleFor(x => x.DepositRequest!)
            .SetValidator(new MicroDepositCryptoResponseValidator())
            .When(x => x.DepositRequest != null);
            
        RuleFor(x => x.EstimatedProfit!)
            .SetValidator(new EstimatedProfitProjectionValidator())
            .When(x => x.EstimatedProfit != null);
            
        RuleFor(x => x.TotalFees)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total fees must be greater than or equal to 0");
            
        RuleForEach(x => x.Sales)
            .SetValidator(new ListingSaleValidator());
    }
}
