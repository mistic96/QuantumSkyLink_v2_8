using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="PublicTeamExtendedPrice"/>
/// </summary>
public class PublicTeamExtendedPriceValidator : AbstractValidator<PublicTeamExtendedPrice>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublicTeamExtendedPriceValidator"/> class
    /// </summary>
    public PublicTeamExtendedPriceValidator()
    {
        RuleFor(x => x.Team!)
            .SetValidator(new PublicTeamValidator())
            .When(x => x.Team != null);
            
        RuleFor(x => x.Price!)
            .SetValidator(new TeamPriceValidator())
            .When(x => x.Price != null);
    }
}
