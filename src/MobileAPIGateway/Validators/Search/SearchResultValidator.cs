using FluentValidation;
using MobileAPIGateway.Models.Search;

namespace MobileAPIGateway.Validators.Search;

/// <summary>
/// Validator for the SearchResult model
/// </summary>
public class SearchResultValidator : AbstractValidator<SearchResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchResultValidator"/> class
    /// </summary>
    public SearchResultValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");
        
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required");
        
        RuleFor(x => x.CreatedAt)
            .NotEmpty().WithMessage("Created date is required");
        
        RuleFor(x => x.UpdatedAt)
            .NotEmpty().WithMessage("Updated date is required");
    }
}
