using FluentValidation;
using MobileAPIGateway.Models.Wallet;

namespace MobileAPIGateway.Validators.Wallet;

/// <summary>
/// Validator for the transfer request model
/// </summary>
public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransferRequestValidator"/> class
    /// </summary>
    public TransferRequestValidator()
    {
        RuleFor(x => x.SourceWalletId)
            .NotEmpty().WithMessage("Source wallet ID is required");
        
        RuleFor(x => x.DestinationWalletId)
            .NotEmpty().WithMessage("Destination wallet ID is required")
            .NotEqual(x => x.SourceWalletId).WithMessage("Destination wallet ID cannot be the same as source wallet ID");
        
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount is required")
            .GreaterThan(0).WithMessage("Amount must be greater than 0");
        
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3, 10).WithMessage("Currency code must be between 3 and 10 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
        
        RuleFor(x => x.ReferenceId)
            .MaximumLength(100).WithMessage("Reference ID cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceId));
    }
}
