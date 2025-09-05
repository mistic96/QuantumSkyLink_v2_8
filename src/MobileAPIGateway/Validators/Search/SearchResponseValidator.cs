using FluentValidation;
using MobileAPIGateway.Models.Search;

namespace MobileAPIGateway.Validators.Search;

/// <summary>
/// Validator for the SearchResponse model
/// </summary>
public class SearchResponseValidator : AbstractValidator<SearchResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchResponseValidator"/> class
    /// </summary>
    public SearchResponseValidator()
    {
        RuleFor(x => x.Results)
            .NotNull().WithMessage("Results cannot be null");
        
        RuleForEach(x => x.Results)
            .SetValidator(new SearchResultValidator());
        
        RuleFor(x => x.TotalResults)
            .GreaterThanOrEqualTo(0).WithMessage("Total results must be greater than or equal to 0");
        
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");
        
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0");
        
        RuleFor(x => x.TotalPages)
            .GreaterThanOrEqualTo(0).WithMessage("Total pages must be greater than or equal to 0");
    }
}
