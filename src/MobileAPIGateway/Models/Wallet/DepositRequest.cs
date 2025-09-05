using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Wallet;

/// <summary>
/// Deposit request model with enhanced deposit code validation support
/// </summary>
public class DepositRequest
{
    /// <summary>
    /// Gets or sets the wallet ID
    /// </summary>
    [Required]
    public string WalletId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    [Required]
    public string CurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the blockchain network
    /// </summary>
    [Required]
    public string BlockchainNetwork { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the reference ID
    /// </summary>
    [StringLength(100)]
    public string? ReferenceId { get; set; }
    
    /// <summary>
    /// Gets or sets the deposit code for enhanced security validation
    /// Required for fiat deposits, optional for crypto deposits
    /// </summary>
    [StringLength(8, MinimumLength = 8, ErrorMessage = "Deposit code must be exactly 8 characters")]
    [RegularExpression("^[A-Z0-9]{8}$", ErrorMessage = "Deposit code must contain only uppercase letters and numbers")]
    public string? DepositCode { get; set; }
    
    /// <summary>
    /// Gets or sets the deposit amount (required when using deposit code)
    /// </summary>
    public decimal? Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the payment method type (fiat or crypto)
    /// </summary>
    [Required]
    public DepositPaymentMethod PaymentMethod { get; set; } = DepositPaymentMethod.Crypto;
    
    /// <summary>
    /// Gets or sets whether to auto-generate a deposit code
    /// </summary>
    public bool AutoGenerateCode { get; set; } = false;
    
    /// <summary>
    /// Gets or sets additional metadata for mobile features
    /// </summary>
    public DepositMetadata? Metadata { get; set; }
}

/// <summary>
/// Deposit payment method enumeration
/// </summary>
public enum DepositPaymentMethod
{
    /// <summary>
    /// Cryptocurrency deposit
    /// </summary>
    Crypto = 0,
    
    /// <summary>
    /// Fiat currency deposit
    /// </summary>
    Fiat = 1
}

/// <summary>
/// Deposit metadata for mobile-specific features
/// </summary>
public class DepositMetadata
{
    /// <summary>
    /// Gets or sets whether this deposit is from QR code scan
    /// </summary>
    public bool IsQrCodeScan { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the mobile device ID
    /// </summary>
    public string? DeviceId { get; set; }
    
    /// <summary>
    /// Gets or sets the app version
    /// </summary>
    public string? AppVersion { get; set; }
    
    /// <summary>
    /// Gets or sets the offline mode flag
    /// </summary>
    public bool IsOfflineMode { get; set; } = false;
    
    /// <summary>
    /// Gets or sets biometric verification status
    /// </summary>
    public bool BiometricVerified { get; set; } = false;

    /// <summary>
    /// Optional capture mode hint for fiat: "client" (Square SDK tokenization) or "payment_link" (hosted).
    /// </summary>
    public string? CaptureMode { get; set; }

    /// <summary>
    /// Optional buyer email for sending hosted payment links.
    /// </summary>
    public string? Email { get; set; }
}
