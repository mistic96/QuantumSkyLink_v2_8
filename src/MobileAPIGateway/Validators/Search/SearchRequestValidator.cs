using FluentValidation;
using MobileAPIGateway.Models.Search;

namespace MobileAPIGateway.Validators.Search;

/// <summary>
/// Validator for the SearchRequest model
/// </summary>
public class SearchRequestValidator : AbstractValidator<SearchRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchRequestValidator"/> class
    /// </summary>
    public SearchRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Search query is required");
        
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");
        
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must be less than or equal to 100");
    }
}
