using FluentValidation;
using MobileAPIGateway.Models.PrimaryMarkets;

namespace MobileAPIGateway.Validators.PrimaryMarkets;

/// <summary>
/// Validator for <see cref="TokenIndexProfile"/>
/// </summary>
public class TokenIndexProfileValidator : AbstractValidator<TokenIndexProfile>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenIndexProfileValidator"/> class
    /// </summary>
    public TokenIndexProfileValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.IndexId)
            .NotEmpty()
            .WithMessage("Index ID is required");
            
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name cannot exceed 100 characters");
            
        RuleFor(x => x.Logo)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Logo))
            .WithMessage("Logo URL cannot exceed 1000 characters");
            
        RuleFor(x => x.BasePrice)
            .GreaterThan(0)
            .When(x => x.BasePrice.HasValue)
            .WithMessage("Base price must be greater than 0");
            
        RuleFor(x => x.CurrentPrice)
            .GreaterThan(0)
            .When(x => x.CurrentPrice.HasValue)
            .WithMessage("Current price must be greater than 0");
            
        RuleFor(x => x.MarketSymbol)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.MarketSymbol))
            .WithMessage("Market symbol cannot exceed 50 characters");
            
        RuleFor(x => x.Available)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Available amount must be greater than or equal to 0");
            
        RuleFor(x => x.MaxSupply)
            .GreaterThan(0)
            .When(x => x.MaxSupply.HasValue)
            .WithMessage("Maximum supply must be greater than 0");
            
        RuleFor(x => x.SmallestUnit)
            .GreaterThan(0)
            .WithMessage("Smallest unit must be greater than 0");
            
        RuleFor(x => x.ExchangeRate!)
            .SetValidator(new MarketExchangeRateValidator())
            .When(x => x.ExchangeRate != null);
            
        RuleFor(x => x.Description!)
            .SetValidator(new TokenIndexDescriptionValidator())
            .When(x => x.Description != null);
            
        RuleForEach(x => x.KeyFeatures)
            .SetValidator(new TokenIndexKeyFeaturesValidator())
            .When(x => x.KeyFeatures != null && x.KeyFeatures.Any());
            
        RuleForEach(x => x.MileStones)
            .SetValidator(new TokenIndexMileStoneValidator())
            .When(x => x.MileStones != null && x.MileStones.Any());
            
        RuleForEach(x => x.Gallery)
            .SetValidator(new TokenIndexImageGalleryValidator())
            .When(x => x.Gallery != null && x.Gallery.Any());
            
        RuleFor(x => x.TokenTagLink)
            .MaximumLength(64)
            .When(x => !string.IsNullOrEmpty(x.TokenTagLink))
            .WithMessage("Token tag link cannot exceed 64 characters");
    }
}
