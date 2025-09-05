using FluentValidation;
using MobileAPIGateway.Models.Global;

namespace MobileAPIGateway.Validators.Global;

/// <summary>
/// Validator for the AppConfig model
/// </summary>
public class AppConfigValidator : AbstractValidator<AppConfig>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppConfigValidator"/> class
    /// </summary>
    public AppConfigValidator()
    {
        RuleFor(x => x.AppName)
            .NotEmpty().WithMessage("Application name is required");
        
        RuleFor(x => x.AppVersion)
            .NotEmpty().WithMessage("Application version is required");
        
        RuleFor(x => x.ApiVersion)
            .NotEmpty().WithMessage("API version is required");
        
        RuleFor(x => x.Environment)
            .NotEmpty().WithMessage("Environment is required");
        
        RuleFor(x => x.BuildNumber)
            .NotEmpty().WithMessage("Build number is required");
        
        RuleFor(x => x.BuildDate)
            .NotEmpty().WithMessage("Build date is required");
    }
}
