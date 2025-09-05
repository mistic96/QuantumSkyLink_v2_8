using FluentValidation;
using MobileAPIGateway.Models.Auth;

namespace MobileAPIGateway.Validators.Auth;

/// <summary>
/// Validator for <see cref="PasswordResetOperationRequest"/>
/// </summary>
public class PasswordResetOperationRequestValidator : AbstractValidator<PasswordResetOperationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordResetOperationRequestValidator"/> class
    /// </summary>
    public PasswordResetOperationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}
