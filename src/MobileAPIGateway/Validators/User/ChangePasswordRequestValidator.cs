using FluentValidation;
using MobileAPIGateway.Models.User;

namespace MobileAPIGateway.Validators.User;

/// <summary>
/// Validator for the change password request model
/// </summary>
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordRequestValidator"/> class
    /// </summary>
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");
        
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long")
            .MaximumLength(100).WithMessage("New password must not exceed 100 characters")
            .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("New password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("New password must contain at least one special character");
        
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.NewPassword).WithMessage("The new password and confirmation password do not match");
    }
}
