using FluentValidation;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Validators.Dashboard;

/// <summary>
/// Validator for <see cref="PublicTeam"/>
/// </summary>
public class PublicTeamValidator : AbstractValidator<PublicTeam>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublicTeamValidator"/> class
    /// </summary>
    public PublicTeamValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
            
        RuleFor(x => x.BlockChainAlisa)
            .NotEmpty()
            .WithMessage("Blockchain alias is required")
            .MaximumLength(100)
            .WithMessage("Blockchain alias cannot exceed 100 characters");
            
        RuleFor(x => x.Logo)
            .NotEmpty()
            .WithMessage("Logo is required")
            .MaximumLength(2000)
            .WithMessage("Logo cannot exceed 2000 characters");
            
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");
            
        RuleFor(x => x.Available)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Available must be greater than or equal to 0");
            
        RuleFor(x => x.Assigned)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Assigned must be greater than or equal to 0");
            
        RuleFor(x => x.Reserved)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Reserved.HasValue)
            .WithMessage("Reserved must be greater than or equal to 0");
            
        RuleFor(x => x.MarketSymbol)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.MarketSymbol))
            .WithMessage("Market symbol cannot exceed 50 characters");
    }
}
