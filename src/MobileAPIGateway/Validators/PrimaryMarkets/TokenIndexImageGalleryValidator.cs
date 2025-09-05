using FluentValidation;
using MobileAPIGateway.Models.PrimaryMarkets;

namespace MobileAPIGateway.Validators.PrimaryMarkets;

/// <summary>
/// Validator for <see cref="TokenIndexImageGallery"/>
/// </summary>
public class TokenIndexImageGalleryValidator : AbstractValidator<TokenIndexImageGallery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenIndexImageGalleryValidator"/> class
    /// </summary>
    public TokenIndexImageGalleryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name cannot exceed 100 characters");
            
        RuleFor(x => x.Url)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Url))
            .WithMessage("URL cannot exceed 1000 characters");
            
        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SortOrder.HasValue)
            .WithMessage("Sort order must be greater than or equal to 0");
    }
}
