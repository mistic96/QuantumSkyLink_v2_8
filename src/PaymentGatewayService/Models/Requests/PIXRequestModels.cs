using System.ComponentModel.DataAnnotations;

namespace PaymentGatewayService.Models.Requests;

/// <summary>
/// Request model for creating a PIX charge via API
/// </summary>
public class CreatePIXChargeRequest
{
    /// <summary>
    /// Amount to charge in the smallest currency unit (cents for BRL)
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public int AmountInCents { get; set; }

    /// <summary>
    /// Description for the payment (max 43 characters)
    /// </summary>
    [MaxLength(43)]
    public string? Description { get; set; }

    /// <summary>
    /// Expiration time in seconds (default 3600 - 1 hour)
    /// </summary>
    [Range(300, 604800, ErrorMessage = "Expiration must be between 5 minutes and 7 days")]
    public int? ExpirationSeconds { get; set; }

    /// <summary>
    /// Payer's full name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string PayerName { get; set; } = string.Empty;

    /// <summary>
    /// Payer's CPF or CNPJ document
    /// </summary>
    [Required]
    [RegularExpression(@"^\d{11}$|^\d{14}$", ErrorMessage = "Document must be 11 digits (CPF) or 14 digits (CNPJ)")]
    public string PayerDocument { get; set; } = string.Empty;

    /// <summary>
    /// Payer's email address
    /// </summary>
    [EmailAddress]
    public string? PayerEmail { get; set; }

    /// <summary>
    /// Payer's phone number
    /// </summary>
    [Phone]
    public string? PayerPhone { get; set; }

    /// <summary>
    /// Callback URL for webhook notifications
    /// </summary>
    [Url]
    public string? CallbackUrl { get; set; }

    /// <summary>
    /// Additional metadata for the charge
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Request model for creating a PIX payout via API
/// </summary>
public class CreatePIXPayoutRequest
{
    /// <summary>
    /// Amount to payout in the smallest currency unit (cents for BRL)
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public int AmountInCents { get; set; }

    /// <summary>
    /// Recipient's PIX key
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PixKey { get; set; } = string.Empty;

    /// <summary>
    /// Type of PIX key (cpf, cnpj, email, phone, random)
    /// </summary>
    [Required]
    [RegularExpression("^(cpf|cnpj|email|phone|random)$", ErrorMessage = "Invalid PIX key type")]
    public string PixKeyType { get; set; } = string.Empty;

    /// <summary>
    /// Recipient's full name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// Recipient's CPF or CNPJ document (optional)
    /// </summary>
    [RegularExpression(@"^\d{11}$|^\d{14}$", ErrorMessage = "Document must be 11 digits (CPF) or 14 digits (CNPJ)")]
    public string? RecipientDocument { get; set; }

    /// <summary>
    /// Description for the payout (max 43 characters)
    /// </summary>
    [MaxLength(43)]
    public string? Description { get; set; }

    /// <summary>
    /// Idempotency key to prevent duplicate payouts
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Additional metadata for the payout
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Request model for generating a static PIX QR code
/// </summary>
public class GenerateStaticQRCodeRequest
{
    /// <summary>
    /// Optional fixed amount for the QR code (in cents)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public int? AmountInCents { get; set; }

    /// <summary>
    /// Description for the QR code (max 43 characters)
    /// </summary>
    [MaxLength(43)]
    public string? Description { get; set; }

    /// <summary>
    /// Merchant name to display
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string MerchantName { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Request model for validating a PIX key
/// </summary>
public class ValidatePixKeyRequest
{
    /// <summary>
    /// The PIX key to validate
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PixKey { get; set; } = string.Empty;

    /// <summary>
    /// Type of PIX key (cpf, cnpj, email, phone, random)
    /// </summary>
    [Required]
    [RegularExpression("^(cpf|cnpj|email|phone|random)$", ErrorMessage = "Invalid PIX key type")]
    public string KeyType { get; set; } = string.Empty;
}