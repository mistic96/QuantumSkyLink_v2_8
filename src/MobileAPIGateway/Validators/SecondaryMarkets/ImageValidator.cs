using FluentValidation;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Validators.SecondaryMarkets;

/// <summary>
/// Validator for <see cref="Image"/>
/// </summary>
public class ImageValidator : AbstractValidator<Image>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageValidator"/> class
    /// </summary>
    public ImageValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.Name)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name cannot exceed 255 characters");
            
        RuleFor(x => x.Url)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Url))
            .WithMessage("URL cannot exceed 2000 characters");
    }
}
