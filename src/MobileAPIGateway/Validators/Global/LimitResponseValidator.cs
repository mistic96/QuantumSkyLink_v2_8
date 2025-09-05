using FluentValidation;
using MobileAPIGateway.Models.Global;

namespace MobileAPIGateway.Validators.Global;

/// <summary>
/// Validator for the <see cref="LimitResponse"/> class
/// </summary>
public class LimitResponseValidator : AbstractValidator<LimitResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LimitResponseValidator"/> class
    /// </summary>
    public LimitResponseValidator()
    {
        RuleFor(x => x.DailyLimit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Daily limit must be greater than or equal to 0");
            
        RuleFor(x => x.WeeklyLimit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Weekly limit must be greater than or equal to 0");
            
        RuleFor(x => x.MonthlyLimit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Monthly limit must be greater than or equal to 0");
            
        RuleFor(x => x.DailyUsage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Daily usage must be greater than or equal to 0");
            
        RuleFor(x => x.WeeklyUsage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Weekly usage must be greater than or equal to 0");
            
        RuleFor(x => x.MonthlyUsage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Monthly usage must be greater than or equal to 0");
    }
}
