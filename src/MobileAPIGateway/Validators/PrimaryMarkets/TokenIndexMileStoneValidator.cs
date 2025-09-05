using FluentValidation;
using MobileAPIGateway.Models.PrimaryMarkets;

namespace MobileAPIGateway.Validators.PrimaryMarkets;

/// <summary>
/// Validator for <see cref="TokenIndexMileStone"/>
/// </summary>
public class TokenIndexMileStoneValidator : AbstractValidator<TokenIndexMileStone>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenIndexMileStoneValidator"/> class
    /// </summary>
    public TokenIndexMileStoneValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name cannot exceed 100 characters");
            
        RuleFor(x => x.Details)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Details))
            .WithMessage("Details cannot exceed 1000 characters");
            
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be greater than or equal to start date");
            
        RuleFor(x => x.IndexMileStoneMeasurement!)
            .SetValidator(new TokenIndexMileStoneMeasurementValidator())
            .When(x => x.IndexMileStoneMeasurement != null);
            
        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SortOrder.HasValue)
            .WithMessage("Sort order must be greater than or equal to 0");
    }
}
