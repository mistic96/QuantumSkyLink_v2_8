using FluentValidation;
using MobileAPIGateway.Models.User;

namespace MobileAPIGateway.Validators.User;

/// <summary>
/// Validator for the update user request model
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRequestValidator"/> class
    /// </summary>
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");
        
        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");
        
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Phone number is not valid").When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        
        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters");
        
        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");
        
        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State must not exceed 100 characters");
        
        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");
        
        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");
        
        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past").When(x => x.DateOfBirth.HasValue);
        
        RuleFor(x => x.ProfilePictureUrl)
            .MaximumLength(500).WithMessage("Profile picture URL must not exceed 500 characters")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).WithMessage("Profile picture URL is not valid").When(x => !string.IsNullOrEmpty(x.ProfilePictureUrl));
    }
}
