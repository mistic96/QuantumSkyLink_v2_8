using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="ListingSale"/>
/// </summary>
public class ListingSaleValidator : AbstractValidator<ListingSale>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListingSaleValidator"/> class
    /// </summary>
    public ListingSaleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.ListingId)
            .NotEmpty()
            .WithMessage("Listing ID is required");
            
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");
            
        RuleFor(x => x.TransactionSetId)
            .NotEmpty()
            .WithMessage("Transaction set ID is required");
            
        RuleFor(x => x.BlockReference)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.BlockReference))
            .WithMessage("Block reference cannot exceed 100 characters");
            
        RuleFor(x => x.ExchangeRate!)
            .SetValidator(new LedgerMarketExchangeRateValidator())
            .When(x => x.ExchangeRate != null);
            
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");
            
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0");
            
        RuleFor(x => x.TotalPrice)
            .GreaterThan(0)
            .WithMessage("Total price must be greater than 0");
            
        RuleFor(x => x.TotalFees)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total fees must be greater than or equal to 0");
    }
}
