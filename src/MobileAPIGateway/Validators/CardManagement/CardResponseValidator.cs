using FluentValidation;
using MobileAPIGateway.Models.CardManagement;

namespace MobileAPIGateway.Validators.CardManagement;

/// <summary>
/// Validator for the CardResponse model
/// </summary>
public class CardResponseValidator : AbstractValidator<CardResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CardResponseValidator"/> class
    /// </summary>
    public CardResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required");
        
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required");
        
        RuleFor(x => x.CardId)
            .NotEmpty().WithMessage("Card ID is required");
        
        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required");
        
        When(x => x.Card != null, () =>
        {
            RuleFor(x => x.Card)
                .SetValidator(new PaymentCardValidator());
        });
    }
}
