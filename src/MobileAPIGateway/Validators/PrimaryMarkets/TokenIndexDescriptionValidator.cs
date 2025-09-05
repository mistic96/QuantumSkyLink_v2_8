using FluentValidation;
using MobileAPIGateway.Models.PrimaryMarkets;

namespace MobileAPIGateway.Validators.PrimaryMarkets;

/// <summary>
/// Validator for <see cref="TokenIndexDescription"/>
/// </summary>
public class TokenIndexDescriptionValidator : AbstractValidator<TokenIndexDescription>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenIndexDescriptionValidator"/> class
    /// </summary>
    public TokenIndexDescriptionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.Summary)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Summary))
            .WithMessage("Summary cannot exceed 2000 characters");
    }
}
