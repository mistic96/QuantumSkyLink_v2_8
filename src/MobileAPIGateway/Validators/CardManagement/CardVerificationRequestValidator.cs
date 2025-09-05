using FluentValidation;
using MobileAPIGateway.Models.CardManagement;

namespace MobileAPIGateway.Validators.CardManagement;

/// <summary>
/// Validator for the CardVerificationRequest model
/// </summary>
public class CardVerificationRequestValidator : AbstractValidator<CardVerificationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CardVerificationRequestValidator"/> class
    /// </summary>
    public CardVerificationRequestValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty().WithMessage("Card ID is required");
        
        RuleFor(x => x.VerificationCode)
            .NotEmpty().WithMessage("Verification code is required")
            .Matches(@"^\d+$").WithMessage("Verification code must contain only digits");
        
        RuleFor(x => x.VerificationMethod)
            .NotEmpty().WithMessage("Verification method is required")
            .Must(BeValidVerificationMethod).WithMessage("Invalid verification method");
    }
    
    private bool BeValidVerificationMethod(string method)
    {
        return method == "MicroDeposit" || method == "3DS" || method == "SMS";
    }
}
