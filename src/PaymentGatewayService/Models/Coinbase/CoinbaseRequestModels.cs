using System.Text.Json.Serialization;

namespace PaymentGatewayService.Models.Coinbase;

/// <summary>
/// Request model for creating a new order on Coinbase
/// </summary>
public class CoinbaseOrderRequest
{
    /// <summary>
    /// Product ID for the order (e.g., "BTC-USDC")
    /// </summary>
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Order side: "BUY" or "SELL"
    /// </summary>
    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    /// <summary>
    /// Order type: "MARKET", "LIMIT", "STOP_LIMIT"
    /// </summary>
    [JsonPropertyName("order_configuration")]
    public CoinbaseOrderConfiguration OrderConfiguration { get; set; } = new();

    /// <summary>
    /// Client-provided ID for idempotency
    /// </summary>
    [JsonPropertyName("client_order_id")]
    public string ClientOrderId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Optional self-trade prevention ID
    /// </summary>
    [JsonPropertyName("self_trade_prevention_id")]
    public string? SelfTradePreventionId { get; set; }

    /// <summary>
    /// Optional leverage for margin trading
    /// </summary>
    [JsonPropertyName("leverage")]
    public string? Leverage { get; set; }

    /// <summary>
    /// Optional margin type
    /// </summary>
    [JsonPropertyName("margin_type")]
    public string? MarginType { get; set; }

    /// <summary>
    /// Optional retail portfolio ID
    /// </summary>
    [JsonPropertyName("retail_portfolio_id")]
    public string? RetailPortfolioId { get; set; }
}

/// <summary>
/// Order configuration for different order types
/// </summary>
public class CoinbaseOrderConfiguration
{
    /// <summary>
    /// Market order configuration
    /// </summary>
    [JsonPropertyName("market_market_ioc")]
    public MarketOrderConfig? MarketOrder { get; set; }

    /// <summary>
    /// Limit order configuration
    /// </summary>
    [JsonPropertyName("limit_limit_gtc")]
    public LimitOrderConfig? LimitOrderGTC { get; set; }

    /// <summary>
    /// Limit order with immediate-or-cancel
    /// </summary>
    [JsonPropertyName("limit_limit_ioc")]
    public LimitOrderConfig? LimitOrderIOC { get; set; }

    /// <summary>
    /// Limit order with fill-or-kill
    /// </summary>
    [JsonPropertyName("limit_limit_fok")]
    public LimitOrderConfig? LimitOrderFOK { get; set; }

    /// <summary>
    /// Stop limit order configuration
    /// </summary>
    [JsonPropertyName("stop_limit_stop_limit_gtc")]
    public StopLimitOrderConfig? StopLimitOrderGTC { get; set; }
}

/// <summary>
/// Market order configuration
/// </summary>
public class MarketOrderConfig
{
    /// <summary>
    /// Quote size for BUY orders (amount in quote currency)
    /// </summary>
    [JsonPropertyName("quote_size")]
    public string? QuoteSize { get; set; }

    /// <summary>
    /// Base size for SELL orders (amount in base currency)
    /// </summary>
    [JsonPropertyName("base_size")]
    public string? BaseSize { get; set; }
}

/// <summary>
/// Limit order configuration
/// </summary>
public class LimitOrderConfig
{
    /// <summary>
    /// Limit price
    /// </summary>
    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; } = string.Empty;

    /// <summary>
    /// Order size in base currency
    /// </summary>
    [JsonPropertyName("base_size")]
    public string BaseSize { get; set; } = string.Empty;

    /// <summary>
    /// Post-only flag (maker orders only)
    /// </summary>
    [JsonPropertyName("post_only")]
    public bool PostOnly { get; set; } = false;
}

/// <summary>
/// Stop limit order configuration
/// </summary>
public class StopLimitOrderConfig
{
    /// <summary>
    /// Stop price trigger
    /// </summary>
    [JsonPropertyName("stop_price")]
    public string StopPrice { get; set; } = string.Empty;

    /// <summary>
    /// Limit price after stop triggers
    /// </summary>
    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; } = string.Empty;

    /// <summary>
    /// Order size in base currency
    /// </summary>
    [JsonPropertyName("base_size")]
    public string BaseSize { get; set; } = string.Empty;

    /// <summary>
    /// Stop direction: "STOP_DIRECTION_STOP_UP" or "STOP_DIRECTION_STOP_DOWN"
    /// </summary>
    [JsonPropertyName("stop_direction")]
    public string StopDirection { get; set; } = string.Empty;
}

/// <summary>
/// Request for listing orders
/// </summary>
public class CoinbaseListOrdersRequest
{
    /// <summary>
    /// Filter by product ID
    /// </summary>
    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    /// <summary>
    /// Filter by order status
    /// </summary>
    [JsonPropertyName("order_status")]
    public List<string>? OrderStatus { get; set; }

    /// <summary>
    /// Maximum number of orders to return
    /// </summary>
    [JsonPropertyName("limit")]
    public int Limit { get; set; } = 100;

    /// <summary>
    /// Start time for filtering
    /// </summary>
    [JsonPropertyName("start_date")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End time for filtering
    /// </summary>
    [JsonPropertyName("end_date")]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// User native currency for value calculations
    /// </summary>
    [JsonPropertyName("user_native_currency")]
    public string? UserNativeCurrency { get; set; }

    /// <summary>
    /// Order type filter
    /// </summary>
    [JsonPropertyName("order_type")]
    public string? OrderType { get; set; }

    /// <summary>
    /// Order side filter
    /// </summary>
    [JsonPropertyName("order_side")]
    public string? OrderSide { get; set; }

    /// <summary>
    /// Cursor for pagination
    /// </summary>
    [JsonPropertyName("cursor")]
    public string? Cursor { get; set; }
}

/// <summary>
/// Request for creating a portfolio
/// </summary>
public class CoinbaseCreatePortfolioRequest
{
    /// <summary>
    /// Portfolio name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// WebSocket subscription request
/// </summary>
public class CoinbaseWebSocketSubscribeRequest
{
    /// <summary>
    /// Request type (always "subscribe")
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "subscribe";

    /// <summary>
    /// Product IDs to subscribe to
    /// </summary>
    [JsonPropertyName("product_ids")]
    public List<string> ProductIds { get; set; } = new();

    /// <summary>
    /// Channels to subscribe to
    /// </summary>
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// JWT token for authentication
    /// </summary>
    [JsonPropertyName("jwt")]
    public string? JWT { get; set; }

    /// <summary>
    /// Timestamp for the request
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
}