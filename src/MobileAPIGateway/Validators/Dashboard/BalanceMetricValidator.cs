using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="BalanceMetric"/>
/// </summary>
public class BalanceMetricValidator : AbstractValidator<BalanceMetric>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BalanceMetricValidator"/> class
    /// </summary>
    public BalanceMetricValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.ReviewDate)
            .NotEmpty()
            .WithMessage("Review date is required");
            
        RuleFor(x => x.TotalWalletBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total wallet balance must be greater than or equal to 0");
    }
}
