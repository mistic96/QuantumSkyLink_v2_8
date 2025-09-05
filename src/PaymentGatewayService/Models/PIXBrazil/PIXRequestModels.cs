namespace PaymentGatewayService.Models.PIXBrazil;

/// <summary>
/// Request model for creating a PIX payout (money out)
/// </summary>
public class PIXPayoutRequest
{
    /// <summary>
    /// Amount to be paid in cents (BRL)
    /// </summary>
    public int AmountInCents { get; set; }

    /// <summary>
    /// Currency code (always BRL for PIX)
    /// </summary>
    public string Currency { get; set; } = "BRL";

    /// <summary>
    /// Target PIX key (CPF, email, phone, or random key)
    /// </summary>
    public string TargetPixKey { get; set; } = string.Empty;

    /// <summary>
    /// Type of PIX key: email, cpf, phone, random
    /// </summary>
    public string TargetPixKeyType { get; set; } = string.Empty;

    /// <summary>
    /// Target recipient's CPF or CNPJ document
    /// </summary>
    public string? TargetDocument { get; set; }

    /// <summary>
    /// Target recipient's name
    /// </summary>
    public string TargetName { get; set; } = string.Empty;

    /// <summary>
    /// Unique key to ensure idempotency
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// URL for webhook callbacks
    /// </summary>
    public string? CallbackUrl { get; set; }

    /// <summary>
    /// Payment description (max 43 characters)
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request model for creating a PIX charge (money in)
/// </summary>
public class PIXChargeRequest
{
    /// <summary>
    /// Payment method type (PIX_DYNAMIC_QR or PIX_STATIC_QR)
    /// </summary>
    public string PaymentMethod { get; set; } = "PIX_DYNAMIC_QR";

    /// <summary>
    /// Payment flow type (DIRECT or REDIRECT)
    /// </summary>
    public string PaymentFlow { get; set; } = "DIRECT";

    /// <summary>
    /// Amount to be charged in cents (BRL)
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Currency code (always BRL for PIX)
    /// </summary>
    public string Currency { get; set; } = "BRL";

    /// <summary>
    /// Expiration information for the PIX charge
    /// </summary>
    public PIXExpirationInfo? ExpirationInfo { get; set; }

    /// <summary>
    /// Payer information
    /// </summary>
    public PIXPayer? Payer { get; set; }

    /// <summary>
    /// Payment description (max 43 characters)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL for webhook callbacks
    /// </summary>
    public string? CallbackUrl { get; set; }
}

/// <summary>
/// PIX charge expiration configuration
/// </summary>
public class PIXExpirationInfo
{
    /// <summary>
    /// Expiration time in seconds (between 3600 and 604800 - 1 hour to 7 days)
    /// </summary>
    public int Seconds { get; set; }
}

/// <summary>
/// PIX payer information
/// </summary>
public class PIXPayer
{
    /// <summary>
    /// Payer's full name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Payer's CPF or CNPJ document
    /// </summary>
    public string Document { get; set; } = string.Empty;

    /// <summary>
    /// Payer's email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Payer's phone number
    /// </summary>
    public string? Phone { get; set; }
}