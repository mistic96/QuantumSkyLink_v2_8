using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="News"/>
/// </summary>
public class NewsValidator : AbstractValidator<News>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NewsValidator"/> class
    /// </summary>
    public NewsValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters");
            
        RuleFor(x => x.Report)
            .NotEmpty()
            .WithMessage("Report is required")
            .MaximumLength(10000)
            .WithMessage("Report cannot exceed 10000 characters");
            
        RuleFor(x => x.ImageUrl)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Image URL cannot exceed 2000 characters");
            
        RuleFor(x => x.Source)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Source))
            .WithMessage("Source cannot exceed 100 characters");
            
        RuleFor(x => x.Tag)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Tag))
            .WithMessage("Tag cannot exceed 50 characters");
    }
}
