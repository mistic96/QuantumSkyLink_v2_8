namespace PaymentGatewayService.Models.PIXBrazil;

/// <summary>
/// Response model for PIX transactions
/// </summary>
public class PIXTransactionResponse
{
    /// <summary>
    /// Unique transaction identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Transaction status (pending, processing, completed, failed, cancelled)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// PIX QR code string for payment
    /// </summary>
    public string? QRCode { get; set; }

    /// <summary>
    /// Base64 encoded QR code image
    /// </summary>
    public string? QRCodeBase64 { get; set; }

    /// <summary>
    /// PIX key used in the transaction
    /// </summary>
    public string? PixKey { get; set; }

    /// <summary>
    /// Transaction amount in cents
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Currency code (BRL)
    /// </summary>
    public string Currency { get; set; } = "BRL";

    /// <summary>
    /// Transaction creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Transaction expiration timestamp (for charges)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// External payment reference ID
    /// </summary>
    public string? PaymentReferenceId { get; set; }

    /// <summary>
    /// End-to-end identifier for the PIX transaction
    /// </summary>
    public string? EndToEndId { get; set; }

    /// <summary>
    /// Additional transaction metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Response model for PIX QR code generation
/// </summary>
public class PIXQRCodeResponse
{
    /// <summary>
    /// QR code identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// PIX QR code string
    /// </summary>
    public string QRCode { get; set; } = string.Empty;

    /// <summary>
    /// Base64 encoded QR code image
    /// </summary>
    public string QRCodeBase64 { get; set; } = string.Empty;

    /// <summary>
    /// QR code type (static or dynamic)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Amount configured for the QR code (optional for static)
    /// </summary>
    public int? Amount { get; set; }

    /// <summary>
    /// QR code creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// QR code expiration timestamp (if applicable)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Webhook payload from PIX provider
/// </summary>
public class PIXWebhookPayload
{
    /// <summary>
    /// Webhook event type (payment.completed, payment.failed, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Transaction data associated with the webhook
    /// </summary>
    public PIXTransactionResponse? Data { get; set; }

    /// <summary>
    /// Webhook signature for verification
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Unix timestamp of when the webhook was sent
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// Unique webhook event ID
    /// </summary>
    public string? EventId { get; set; }
}

/// <summary>
/// PIX error response model
/// </summary>
public class PIXErrorResponse
{
    /// <summary>
    /// Error code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error description
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// Field that caused the error (for validation errors)
    /// </summary>
    public string? Field { get; set; }
}