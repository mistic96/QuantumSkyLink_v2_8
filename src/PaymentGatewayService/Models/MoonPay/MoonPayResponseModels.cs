using System.Text.Json.Serialization;

namespace PaymentGatewayService.Models.MoonPay;

/// <summary>
/// Response model for transaction creation
/// </summary>
public class MoonPayTransactionResponse
{
    /// <summary>
    /// Unique transaction ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Transaction status
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Amount of cryptocurrency
    /// </summary>
    [JsonPropertyName("cryptoAmount")]
    public decimal? CryptoAmount { get; set; }

    /// <summary>
    /// Amount in fiat currency
    /// </summary>
    [JsonPropertyName("fiatAmount")]
    public decimal? FiatAmount { get; set; }

    /// <summary>
    /// Cryptocurrency code
    /// </summary>
    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Base currency code (fiat)
    /// </summary>
    [JsonPropertyName("baseCurrencyCode")]
    public string BaseCurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Wallet address for crypto delivery
    /// </summary>
    [JsonPropertyName("walletAddress")]
    public string? WalletAddress { get; set; }

    /// <summary>
    /// Exchange rate applied
    /// </summary>
    [JsonPropertyName("exchangeRate")]
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// Fee amount in base currency
    /// </summary>
    [JsonPropertyName("feeAmount")]
    public decimal? FeeAmount { get; set; }

    /// <summary>
    /// Network fee amount
    /// </summary>
    [JsonPropertyName("networkFeeAmount")]
    public decimal? NetworkFeeAmount { get; set; }

    /// <summary>
    /// External transaction ID
    /// </summary>
    [JsonPropertyName("externalTransactionId")]
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// Widget URL for completing transaction
    /// </summary>
    [JsonPropertyName("widgetUrl")]
    public string? WidgetUrl { get; set; }

    /// <summary>
    /// Transaction creation time
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update time
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Failure reason if status is failed
    /// </summary>
    [JsonPropertyName("failureReason")]
    public string? FailureReason { get; set; }

    /// <summary>
    /// Payment method used
    /// </summary>
    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Customer information
    /// </summary>
    [JsonPropertyName("customer")]
    public MoonPayCustomer? Customer { get; set; }
}

/// <summary>
/// Customer information in transaction
/// </summary>
public class MoonPayCustomer
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("isVerified")]
    public bool IsVerified { get; set; }
}

/// <summary>
/// Transaction status details
/// </summary>
public class MoonPayTransactionStatus
{
    /// <summary>
    /// Transaction ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Current status
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Status description
    /// </summary>
    [JsonPropertyName("statusDescription")]
    public string? StatusDescription { get; set; }

    /// <summary>
    /// Transaction stages
    /// </summary>
    [JsonPropertyName("stages")]
    public List<MoonPayTransactionStage> Stages { get; set; } = new();

    /// <summary>
    /// Cryptocurrency transaction hash
    /// </summary>
    [JsonPropertyName("cryptoTransactionId")]
    public string? CryptoTransactionId { get; set; }

    /// <summary>
    /// Estimated completion time
    /// </summary>
    [JsonPropertyName("estimatedArrivalTime")]
    public DateTime? EstimatedArrivalTime { get; set; }
}

/// <summary>
/// Transaction processing stage
/// </summary>
public class MoonPayTransactionStage
{
    [JsonPropertyName("stage")]
    public string Stage { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("actions")]
    public List<MoonPayAction> Actions { get; set; } = new();

    [JsonPropertyName("failureReason")]
    public string? FailureReason { get; set; }
}

/// <summary>
/// Required action for transaction
/// </summary>
public class MoonPayAction
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// Supported cryptocurrency information
/// </summary>
public class MoonPayCurrency
{
    /// <summary>
    /// Currency code (BTC, ETH, etc.)
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Full currency name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Currency type (crypto, fiat)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Minimum purchase amount in USD
    /// </summary>
    [JsonPropertyName("minBuyAmount")]
    public decimal? MinBuyAmount { get; set; }

    /// <summary>
    /// Maximum purchase amount in USD
    /// </summary>
    [JsonPropertyName("maxBuyAmount")]
    public decimal? MaxBuyAmount { get; set; }

    /// <summary>
    /// Supported payment methods
    /// </summary>
    [JsonPropertyName("supportedPaymentMethods")]
    public List<string> SupportedPaymentMethods { get; set; } = new();

    /// <summary>
    /// Whether selling is supported
    /// </summary>
    [JsonPropertyName("isSellSupported")]
    public bool IsSellSupported { get; set; }

    /// <summary>
    /// Network/blockchain for crypto
    /// </summary>
    [JsonPropertyName("network")]
    public string? Network { get; set; }

    /// <summary>
    /// Contract address for tokens
    /// </summary>
    [JsonPropertyName("contractAddress")]
    public string? ContractAddress { get; set; }
}

/// <summary>
/// Price quote response
/// </summary>
public class MoonPayQuote
{
    /// <summary>
    /// Base currency amount
    /// </summary>
    [JsonPropertyName("baseCurrencyAmount")]
    public decimal BaseCurrencyAmount { get; set; }

    /// <summary>
    /// Quote currency amount
    /// </summary>
    [JsonPropertyName("quoteCurrencyAmount")]
    public decimal QuoteCurrencyAmount { get; set; }

    /// <summary>
    /// Exchange rate
    /// </summary>
    [JsonPropertyName("exchangeRate")]
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// Total fee amount
    /// </summary>
    [JsonPropertyName("totalFee")]
    public decimal TotalFee { get; set; }

    /// <summary>
    /// Network fee
    /// </summary>
    [JsonPropertyName("networkFee")]
    public decimal NetworkFee { get; set; }

    /// <summary>
    /// Quote expiration time
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Widget session response
/// </summary>
public class MoonPayWidgetResponse
{
    /// <summary>
    /// Widget URL to embed or redirect to
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Session ID
    /// </summary>
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Session expiration time
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// KYC/AML screening result
/// </summary>
public class MoonPayScreeningResult
{
    /// <summary>
    /// Whether the recipient passed screening
    /// </summary>
    [JsonPropertyName("isPassed")]
    public bool IsPassed { get; set; }

    /// <summary>
    /// Risk level (low, medium, high)
    /// </summary>
    [JsonPropertyName("riskLevel")]
    public string? RiskLevel { get; set; }

    /// <summary>
    /// Rejection reasons if failed
    /// </summary>
    [JsonPropertyName("rejectionReasons")]
    public List<string> RejectionReasons { get; set; } = new();

    /// <summary>
    /// Required verification level
    /// </summary>
    [JsonPropertyName("requiredVerificationLevel")]
    public string? RequiredVerificationLevel { get; set; }
}

/// <summary>
/// Transaction limits and fees
/// </summary>
public class MoonPayLimits
{
    /// <summary>
    /// Minimum transaction amount
    /// </summary>
    [JsonPropertyName("minAmount")]
    public decimal MinAmount { get; set; }

    /// <summary>
    /// Maximum transaction amount
    /// </summary>
    [JsonPropertyName("maxAmount")]
    public decimal MaxAmount { get; set; }

    /// <summary>
    /// Daily limit
    /// </summary>
    [JsonPropertyName("dailyLimit")]
    public decimal? DailyLimit { get; set; }

    /// <summary>
    /// Monthly limit
    /// </summary>
    [JsonPropertyName("monthlyLimit")]
    public decimal? MonthlyLimit { get; set; }

    /// <summary>
    /// Fee percentage
    /// </summary>
    [JsonPropertyName("feePercentage")]
    public decimal FeePercentage { get; set; }

    /// <summary>
    /// Minimum fee amount
    /// </summary>
    [JsonPropertyName("minFee")]
    public decimal MinFee { get; set; }

    /// <summary>
    /// Processing time estimate
    /// </summary>
    [JsonPropertyName("processingTime")]
    public string? ProcessingTime { get; set; }
}

/// <summary>
/// Webhook payload from MoonPay
/// </summary>
public class MoonPayWebhookPayload
{
    /// <summary>
    /// Webhook event type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Transaction data
    /// </summary>
    [JsonPropertyName("data")]
    public MoonPayWebhookData Data { get; set; } = new();

    /// <summary>
    /// Event ID
    /// </summary>
    [JsonPropertyName("eventId")]
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Event timestamp
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Webhook event data
/// </summary>
public class MoonPayWebhookData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("externalTransactionId")]
    public string? ExternalTransactionId { get; set; }

    [JsonPropertyName("cryptoTransactionId")]
    public string? CryptoTransactionId { get; set; }

    [JsonPropertyName("failureReason")]
    public string? FailureReason { get; set; }

    [JsonPropertyName("cryptoAmount")]
    public decimal? CryptoAmount { get; set; }

    [JsonPropertyName("fiatAmount")]
    public decimal? FiatAmount { get; set; }

    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("baseCurrencyCode")]
    public string? BaseCurrencyCode { get; set; }
}