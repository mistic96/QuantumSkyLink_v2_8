using FluentValidation;
using MobileAPIGateway.Models.Wallet;

namespace MobileAPIGateway.Validators.Wallet;

/// <summary>
/// Validator for deposit code generation requests
/// </summary>
public class DepositCodeGenerationRequestValidator : AbstractValidator<DepositCodeGenerationRequest>
{
    public DepositCodeGenerationRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .When(x => x.Amount.HasValue)
            .WithMessage("Amount must be greater than zero when specified");

        RuleFor(x => x.Currency)
            .Length(3)
            .Matches("^[A-Z]{3}$")
            .When(x => !string.IsNullOrEmpty(x.Currency))
            .WithMessage("Currency must be a valid 3-letter ISO code");

        RuleFor(x => x.ExpirationHours)
            .InclusiveBetween(1, 168)
            .WithMessage("Expiration must be between 1 and 168 hours (7 days)");

        RuleFor(x => x.Metadata)
            .SetValidator(new DepositMetadataValidator()!)
            .When(x => x.Metadata != null);
    }
}

/// <summary>
/// Validator for deposit code validation requests
/// </summary>
public class DepositCodeValidationRequestValidator : AbstractValidator<DepositCodeValidationRequest>
{
    public DepositCodeValidationRequestValidator()
    {
        RuleFor(x => x.DepositCode)
            .NotEmpty()
            .WithMessage("Deposit code is required")
            .Length(8)
            .WithMessage("Deposit code must be exactly 8 characters")
            .Matches("^[A-Z0-9]{8}$")
            .WithMessage("Deposit code must contain only uppercase letters and numbers");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be exactly 3 characters")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be a valid 3-letter ISO code");
    }
}

/// <summary>
/// Validator for enhanced deposit requests
/// </summary>
public class EnhancedDepositRequestValidator : AbstractValidator<EnhancedDepositRequest>
{
    public EnhancedDepositRequestValidator()
    {
        // Include base DepositRequest validation
        RuleFor(x => x.WalletId)
            .NotEmpty()
            .WithMessage("Wallet ID is required");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .WithMessage("Currency code is required")
            .Length(3)
            .WithMessage("Currency code must be exactly 3 characters")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency code must be a valid 3-letter ISO code");

        RuleFor(x => x.BlockchainNetwork)
            .NotEmpty()
            .WithMessage("Blockchain network is required");

        RuleFor(x => x.ReferenceId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.ReferenceId))
            .WithMessage("Reference ID cannot exceed 100 characters");

        // Deposit code validation
        RuleFor(x => x.DepositCode)
            .Length(8)
            .Matches("^[A-Z0-9]{8}$")
            .When(x => !string.IsNullOrEmpty(x.DepositCode))
            .WithMessage("Deposit code must be 8 alphanumeric characters");

        // Amount validation when deposit code is provided
        RuleFor(x => x.Amount)
            .NotNull()
            .GreaterThan(0)
            .When(x => !string.IsNullOrEmpty(x.DepositCode))
            .WithMessage("Amount is required and must be greater than zero when using a deposit code");

        // Fiat deposits require deposit code or auto-generation
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.DepositCode) || x.AutoGenerateCode)
            .When(x => x.PaymentMethod == DepositPaymentMethod.Fiat)
            .WithMessage("Fiat deposits require either a deposit code or auto-generation enabled");

        RuleFor(x => x.Metadata)
            .SetValidator(new DepositMetadataValidator()!)
            .When(x => x.Metadata != null);

        RuleFor(x => x.ClientIpAddress)
            .Matches(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$|^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$")
            .When(x => !string.IsNullOrEmpty(x.ClientIpAddress))
            .WithMessage("Client IP address must be a valid IPv4 or IPv6 address");

        RuleFor(x => x.UserAgent)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.UserAgent))
            .WithMessage("User agent cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for deposit metadata
/// </summary>
public class DepositMetadataValidator : AbstractValidator<DepositMetadata>
{
    public DepositMetadataValidator()
    {
        RuleFor(x => x.DeviceId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.DeviceId))
            .WithMessage("Device ID cannot exceed 100 characters");

        RuleFor(x => x.AppVersion)
            .MaximumLength(20)
            .Matches(@"^\d+\.\d+\.\d+(\.\d+)?$")
            .When(x => !string.IsNullOrEmpty(x.AppVersion))
            .WithMessage("App version must be in format X.Y.Z or X.Y.Z.W");
    }
}

/// <summary>
/// Validator for deposit pre-validation requests
/// </summary>
public class DepositPreValidationRequestValidator : AbstractValidator<DepositPreValidationRequest>
{
    public DepositPreValidationRequestValidator()
    {
        RuleFor(x => x.DepositCode)
            .NotEmpty()
            .WithMessage("Deposit code is required")
            .Length(8)
            .WithMessage("Deposit code must be exactly 8 characters")
            .Matches("^[A-Z0-9]{8}$")
            .WithMessage("Deposit code must contain only uppercase letters and numbers");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be exactly 3 characters")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be a valid 3-letter ISO code");
    }
}


/// <summary>
/// Validator for mobile features configuration
/// </summary>
public class MobileFeaturesValidator : AbstractValidator<MobileFeatures>
{
    public MobileFeaturesValidator()
    {
        RuleFor(x => x.DisplayFormat)
            .NotEmpty()
            .When(x => x.DisplayFormat != null)
            .WithMessage("Display format cannot be empty when specified");

        RuleFor(x => x.PushSettings)
            .SetValidator(new PushNotificationSettingsValidator()!)
            .When(x => x.PushSettings != null);
    }
}

/// <summary>
/// Validator for push notification settings
/// </summary>
public class PushNotificationSettingsValidator : AbstractValidator<PushNotificationSettings>
{
    public PushNotificationSettingsValidator()
    {
        RuleFor(x => x.ExpirationWarningMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(1440) // 24 hours
            .WithMessage("Expiration warning must be between 1 and 1440 minutes");
    }
}
