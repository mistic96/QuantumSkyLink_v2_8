using FluentValidation;
using MobileAPIGateway.Models.Global;

namespace MobileAPIGateway.Validators.Global;

/// <summary>
/// Validator for the SystemStatus model
/// </summary>
public class SystemStatusValidator : AbstractValidator<SystemStatus>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SystemStatusValidator"/> class
    /// </summary>
    public SystemStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required");
        
        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Version is required");
        
        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required");
        
        RuleForEach(x => x.Services.Values)
            .SetValidator(new ServiceStatusValidator());
    }
}

/// <summary>
/// Validator for the ServiceStatus model
/// </summary>
public class ServiceStatusValidator : AbstractValidator<ServiceStatus>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceStatusValidator"/> class
    /// </summary>
    public ServiceStatusValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");
        
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required");
        
        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Version is required");
        
        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required");
    }
}
