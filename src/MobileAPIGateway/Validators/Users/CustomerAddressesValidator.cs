using FluentValidation;
using MobileAPIGateway.Models.Users;

namespace MobileAPIGateway.Validators.Users;

/// <summary>
/// Validator for <see cref="CustomerAddresses"/>
/// </summary>
public class CustomerAddressesValidator : AbstractValidator<CustomerAddresses>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerAddressesValidator"/> class
    /// </summary>
    public CustomerAddressesValidator()
    {
        RuleFor(x => x.StreetAddress)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.StreetAddress))
            .WithMessage("Street address cannot exceed 255 characters");
            
        RuleFor(x => x.AdditionalAddressInfo)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.AdditionalAddressInfo))
            .WithMessage("Additional address information cannot exceed 255 characters");
            
        RuleFor(x => x.StateProvinceRegion)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.StateProvinceRegion))
            .WithMessage("State/province/region cannot exceed 100 characters");
            
        RuleFor(x => x.City)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.City))
            .WithMessage("City cannot exceed 100 characters");
            
        RuleFor(x => x.ZipPostal)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.ZipPostal))
            .WithMessage("ZIP/postal code cannot exceed 20 characters");
            
        RuleFor(x => x.Country!)
            .SetValidator(new CountryValidator())
            .When(x => x.Country != null);
    }
}
