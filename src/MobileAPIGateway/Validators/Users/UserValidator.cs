using FluentValidation;
using MobileAPIGateway.Models.Users;

namespace MobileAPIGateway.Validators.Users;

/// <summary>
/// Validator for <see cref="Models.Users.User"/>
/// </summary>
public class UserValidator : AbstractValidator<Models.Users.User>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserValidator"/> class
    /// </summary>
    public UserValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User ID is required");
            
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("A valid email address is required");
            
        RuleFor(x => x.FirstName)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.FirstName))
            .WithMessage("First name cannot exceed 100 characters");
            
        RuleFor(x => x.LastName)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.LastName))
            .WithMessage("Last name cannot exceed 100 characters");
            
        RuleFor(x => x.MobilePhone)
            .Matches(@"^\+?[0-9\s\-\(\)]+$").When(x => !string.IsNullOrEmpty(x.MobilePhone))
            .WithMessage("Mobile phone must be a valid phone number");
            
        RuleFor(x => x.WalletAddress)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.WalletAddress))
            .WithMessage("Wallet address cannot exceed 255 characters");
            
        RuleFor(x => x.EscrowWalletAddress)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.EscrowWalletAddress))
            .WithMessage("Escrow wallet address cannot exceed 255 characters");
            
        RuleFor(x => x.MultiFactorAuthenticationAmount)
            .GreaterThanOrEqualTo(0).When(x => x.MultiFactorAuthenticationAmount.HasValue)
            .WithMessage("Multi-factor authentication amount must be greater than or equal to 0");
    }
}
