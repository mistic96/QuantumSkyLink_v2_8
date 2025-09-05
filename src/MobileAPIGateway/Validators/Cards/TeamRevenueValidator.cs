using FluentValidation;
using MobileAPIGateway.Models.Cards;

namespace MobileAPIGateway.Validators.Cards;

/// <summary>
/// Validator for <see cref="TeamRevenue"/>
/// </summary>
public class TeamRevenueValidator : AbstractValidator<TeamRevenue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TeamRevenueValidator"/> class
    /// </summary>
    public TeamRevenueValidator()
    {
        RuleFor(x => x.TeamId)
            .GreaterThan(0)
            .WithMessage("Team ID must be greater than 0");
            
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be greater than or equal to 0");
    }
}
