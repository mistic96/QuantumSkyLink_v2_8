using System.Text.Json.Serialization;

namespace PaymentGatewayService.Models.Coinbase;

/// <summary>
/// Coinbase webhook payload
/// </summary>
public class CoinbaseWebhookPayload
{
    /// <summary>
    /// Unique identifier for the webhook notification
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Resource type (e.g., "event")
    /// </summary>
    [JsonPropertyName("resource")]
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// Event type (e.g., "orders.filled", "orders.cancelled", "orders.updated")
    /// </summary>
    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }

    /// <summary>
    /// Event data - dynamic based on event type
    /// </summary>
    [JsonPropertyName("data")]
    public Dictionary<string, object>? Data { get; set; }

    /// <summary>
    /// User ID associated with the event
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// Time the event was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// API version
    /// </summary>
    [JsonPropertyName("api_version")]
    public string? ApiVersion { get; set; }
}

/// <summary>
/// Coinbase order webhook data
/// </summary>
public class CoinbaseOrderWebhookData
{
    /// <summary>
    /// Order ID
    /// </summary>
    [JsonPropertyName("order_id")]
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Client order ID
    /// </summary>
    [JsonPropertyName("client_order_id")]
    public string? ClientOrderId { get; set; }

    /// <summary>
    /// Product ID (e.g., "BTC-USDC")
    /// </summary>
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Order side (BUY or SELL)
    /// </summary>
    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    /// <summary>
    /// Order status
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Order type
    /// </summary>
    [JsonPropertyName("order_type")]
    public string OrderType { get; set; } = string.Empty;

    /// <summary>
    /// Filled size
    /// </summary>
    [JsonPropertyName("filled_size")]
    public string? FilledSize { get; set; }

    /// <summary>
    /// Average filled price
    /// </summary>
    [JsonPropertyName("average_filled_price")]
    public string? AverageFilledPrice { get; set; }

    /// <summary>
    /// Total fees
    /// </summary>
    [JsonPropertyName("total_fees")]
    public string? TotalFees { get; set; }

    /// <summary>
    /// Reject reason if order was rejected
    /// </summary>
    [JsonPropertyName("reject_reason")]
    public string? RejectReason { get; set; }

    /// <summary>
    /// Creation time
    /// </summary>
    [JsonPropertyName("created_time")]
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// Last fill time
    /// </summary>
    [JsonPropertyName("last_fill_time")]
    public DateTime? LastFillTime { get; set; }
}

/// <summary>
/// Coinbase account webhook data
/// </summary>
public class CoinbaseAccountWebhookData
{
    /// <summary>
    /// Account UUID
    /// </summary>
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// Currency
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Available balance
    /// </summary>
    [JsonPropertyName("available_balance")]
    public CoinbaseBalance AvailableBalance { get; set; } = new();

    /// <summary>
    /// Hold balance
    /// </summary>
    [JsonPropertyName("hold")]
    public CoinbaseBalance Hold { get; set; } = new();

    /// <summary>
    /// Account type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Last update time
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}