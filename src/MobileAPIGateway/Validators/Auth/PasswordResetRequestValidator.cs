using FluentValidation;
using MobileAPIGateway.Models.Auth;

namespace MobileAPIGateway.Validators.Auth;

/// <summary>
/// Validator for <see cref="PasswordResetRequest"/>
/// </summary>
public class PasswordResetRequestValidator : AbstractValidator<PasswordResetRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordResetRequestValidator"/> class
    /// </summary>
    public PasswordResetRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required");
    }
}
