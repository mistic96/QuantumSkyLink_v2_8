using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="DynamicUserCoin"/>
/// </summary>
public class DynamicUserCoinValidator : AbstractValidator<DynamicUserCoin>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicUserCoinValidator"/> class
    /// </summary>
    public DynamicUserCoinValidator()
    {
        RuleFor(x => x.TeamName)
            .NotEmpty()
            .WithMessage("Team name is required")
            .MaximumLength(100)
            .WithMessage("Team name cannot exceed 100 characters");
            
        RuleFor(x => x.Logo)
            .NotEmpty()
            .WithMessage("Logo is required")
            .MaximumLength(2000)
            .WithMessage("Logo cannot exceed 2000 characters");
            
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be greater than or equal to 0");
            
        RuleFor(x => x.AverageCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Average cost must be greater than or equal to 0");
            
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be greater than or equal to 0");
            
        RuleFor(x => x.CurrentTokenPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Current token price must be greater than or equal to 0");
    }
}
