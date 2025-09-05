namespace PaymentGatewayService.Models.Responses;

/// <summary>
/// Response model for PIX charge creation
/// </summary>
public class PIXChargeResponse
{
    /// <summary>
    /// Unique identifier for the charge
    /// </summary>
    public string ChargeId { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the charge
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// PIX QR code string (copy and paste format)
    /// </summary>
    public string QRCodeString { get; set; } = string.Empty;

    /// <summary>
    /// Base64 encoded QR code image
    /// </summary>
    public string QRCodeImage { get; set; } = string.Empty;

    /// <summary>
    /// Amount in cents
    /// </summary>
    public int AmountInCents { get; set; }

    /// <summary>
    /// Currency code (BRL)
    /// </summary>
    public string Currency { get; set; } = "BRL";

    /// <summary>
    /// When the charge expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the charge was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Payment description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Response model for PIX payout creation
/// </summary>
public class PIXPayoutResponse
{
    /// <summary>
    /// Unique identifier for the payout
    /// </summary>
    public string PayoutId { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the payout
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Amount in cents
    /// </summary>
    public int AmountInCents { get; set; }

    /// <summary>
    /// Currency code (BRL)
    /// </summary>
    public string Currency { get; set; } = "BRL";

    /// <summary>
    /// Recipient's PIX key
    /// </summary>
    public string RecipientPixKey { get; set; } = string.Empty;

    /// <summary>
    /// Recipient's name
    /// </summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// End-to-end identifier for tracking
    /// </summary>
    public string? EndToEndId { get; set; }

    /// <summary>
    /// When the payout was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the payout was completed (if applicable)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Error message if payout failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Response model for PIX transaction status
/// </summary>
public class PIXTransactionStatusResponse
{
    /// <summary>
    /// Transaction identifier
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Transaction type (charge or payout)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Current status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Amount in cents
    /// </summary>
    public int AmountInCents { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = "BRL";

    /// <summary>
    /// When the transaction was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the transaction was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// QR code data (for charges only)
    /// </summary>
    public PIXQRCodeData? QRCode { get; set; }

    /// <summary>
    /// Payer information (for completed charges)
    /// </summary>
    public PIXPayerInfo? Payer { get; set; }

    /// <summary>
    /// Recipient information (for payouts)
    /// </summary>
    public PIXRecipientInfo? Recipient { get; set; }

    /// <summary>
    /// Transaction events history
    /// </summary>
    public List<PIXTransactionEvent> Events { get; set; } = new();
}

/// <summary>
/// QR code data for PIX charges
/// </summary>
public class PIXQRCodeData
{
    /// <summary>
    /// QR code string
    /// </summary>
    public string QRCodeString { get; set; } = string.Empty;

    /// <summary>
    /// Base64 encoded QR code image
    /// </summary>
    public string QRCodeImage { get; set; } = string.Empty;

    /// <summary>
    /// When the QR code expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Payer information for completed PIX charges
/// </summary>
public class PIXPayerInfo
{
    /// <summary>
    /// Payer's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Payer's document (CPF/CNPJ)
    /// </summary>
    public string Document { get; set; } = string.Empty;

    /// <summary>
    /// Bank that processed the payment
    /// </summary>
    public string? Bank { get; set; }

    /// <summary>
    /// Agency number
    /// </summary>
    public string? Agency { get; set; }

    /// <summary>
    /// Account number
    /// </summary>
    public string? Account { get; set; }
}

/// <summary>
/// Recipient information for PIX payouts
/// </summary>
public class PIXRecipientInfo
{
    /// <summary>
    /// Recipient's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Recipient's PIX key
    /// </summary>
    public string PixKey { get; set; } = string.Empty;

    /// <summary>
    /// PIX key type
    /// </summary>
    public string PixKeyType { get; set; } = string.Empty;

    /// <summary>
    /// Recipient's document (if available)
    /// </summary>
    public string? Document { get; set; }
}

/// <summary>
/// Transaction event in the history
/// </summary>
public class PIXTransactionEvent
{
    /// <summary>
    /// Event type
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Event description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Additional event data
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Response model for static QR code generation
/// </summary>
public class PIXStaticQRCodeResponse
{
    /// <summary>
    /// QR code identifier
    /// </summary>
    public string QRCodeId { get; set; } = string.Empty;

    /// <summary>
    /// PIX QR code string (copy and paste format)
    /// </summary>
    public string QRCodeString { get; set; } = string.Empty;

    /// <summary>
    /// Base64 encoded QR code image
    /// </summary>
    public string QRCodeImage { get; set; } = string.Empty;

    /// <summary>
    /// Fixed amount (if specified)
    /// </summary>
    public int? AmountInCents { get; set; }

    /// <summary>
    /// Merchant name
    /// </summary>
    public string MerchantName { get; set; } = string.Empty;

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the QR code was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Response model for PIX key validation
/// </summary>
public class ValidatePixKeyResponse
{
    /// <summary>
    /// Whether the PIX key is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// The PIX key that was validated
    /// </summary>
    public string PixKey { get; set; } = string.Empty;

    /// <summary>
    /// The key type
    /// </summary>
    public string KeyType { get; set; } = string.Empty;

    /// <summary>
    /// Validation message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional validation details
    /// </summary>
    public Dictionary<string, string>? Details { get; set; }
}