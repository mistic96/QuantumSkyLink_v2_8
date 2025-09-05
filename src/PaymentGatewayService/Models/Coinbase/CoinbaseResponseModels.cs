using System.Text.Json.Serialization;

namespace PaymentGatewayService.Models.Coinbase;

/// <summary>
/// Response model for an order
/// </summary>
public class CoinbaseOrderResponse
{
    /// <summary>
    /// Unique order ID
    /// </summary>
    [JsonPropertyName("order_id")]
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Product ID (e.g., "BTC-USDC")
    /// </summary>
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// User ID who placed the order
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Order side: "BUY" or "SELL"
    /// </summary>
    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    /// <summary>
    /// Client-provided order ID
    /// </summary>
    [JsonPropertyName("client_order_id")]
    public string ClientOrderId { get; set; } = string.Empty;

    /// <summary>
    /// Order status
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Time in force
    /// </summary>
    [JsonPropertyName("time_in_force")]
    public string TimeInForce { get; set; } = string.Empty;

    /// <summary>
    /// Order creation time
    /// </summary>
    [JsonPropertyName("created_time")]
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// Completion percentage
    /// </summary>
    [JsonPropertyName("completion_percentage")]
    public string CompletionPercentage { get; set; } = "0";

    /// <summary>
    /// Filled size
    /// </summary>
    [JsonPropertyName("filled_size")]
    public string FilledSize { get; set; } = "0";

    /// <summary>
    /// Average filled price
    /// </summary>
    [JsonPropertyName("average_filled_price")]
    public string AverageFilledPrice { get; set; } = "0";

    /// <summary>
    /// Total fees
    /// </summary>
    [JsonPropertyName("fee")]
    public string Fee { get; set; } = "0";

    /// <summary>
    /// Number of fills
    /// </summary>
    [JsonPropertyName("number_of_fills")]
    public string NumberOfFills { get; set; } = "0";

    /// <summary>
    /// Filled value in quote currency
    /// </summary>
    [JsonPropertyName("filled_value")]
    public string FilledValue { get; set; } = "0";

    /// <summary>
    /// Whether order is pending cancel
    /// </summary>
    [JsonPropertyName("pending_cancel")]
    public bool PendingCancel { get; set; }

    /// <summary>
    /// Size in quote currency (for market buys)
    /// </summary>
    [JsonPropertyName("size_in_quote")]
    public bool SizeInQuote { get; set; }

    /// <summary>
    /// Total fees in quote currency
    /// </summary>
    [JsonPropertyName("total_fees")]
    public string TotalFees { get; set; } = "0";

    /// <summary>
    /// Total value after fees
    /// </summary>
    [JsonPropertyName("total_value_after_fees")]
    public string TotalValueAfterFees { get; set; } = "0";

    /// <summary>
    /// Trigger status for stop orders
    /// </summary>
    [JsonPropertyName("trigger_status")]
    public string? TriggerStatus { get; set; }

    /// <summary>
    /// Order type
    /// </summary>
    [JsonPropertyName("order_type")]
    public string OrderType { get; set; } = string.Empty;

    /// <summary>
    /// Reject reason if order was rejected
    /// </summary>
    [JsonPropertyName("reject_reason")]
    public string? RejectReason { get; set; }

    /// <summary>
    /// Order configuration
    /// </summary>
    [JsonPropertyName("order_configuration")]
    public CoinbaseOrderConfiguration OrderConfiguration { get; set; } = new();

    /// <summary>
    /// Whether order is liquidation
    /// </summary>
    [JsonPropertyName("is_liquidation")]
    public bool IsLiquidation { get; set; }

    /// <summary>
    /// Last fill time
    /// </summary>
    [JsonPropertyName("last_fill_time")]
    public DateTime? LastFillTime { get; set; }

    /// <summary>
    /// Order placement source
    /// </summary>
    [JsonPropertyName("order_placement_source")]
    public string? OrderPlacementSource { get; set; }
}

/// <summary>
/// Account information
/// </summary>
public class CoinbaseAccount
{
    /// <summary>
    /// Account UUID
    /// </summary>
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// Account name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Currency code
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Available balance
    /// </summary>
    [JsonPropertyName("available_balance")]
    public CoinbaseBalance AvailableBalance { get; set; } = new();

    /// <summary>
    /// Default account flag
    /// </summary>
    [JsonPropertyName("default")]
    public bool Default { get; set; }

    /// <summary>
    /// Account active status
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    /// <summary>
    /// Account creation time
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Account update time
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Whether account can be deleted
    /// </summary>
    [JsonPropertyName("deletable")]
    public bool Deletable { get; set; }

    /// <summary>
    /// Account type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether account is ready
    /// </summary>
    [JsonPropertyName("ready")]
    public bool Ready { get; set; }

    /// <summary>
    /// Hold balance
    /// </summary>
    [JsonPropertyName("hold")]
    public CoinbaseBalance Hold { get; set; } = new();
}

/// <summary>
/// Balance information
/// </summary>
public class CoinbaseBalance
{
    /// <summary>
    /// Balance value
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = "0";

    /// <summary>
    /// Currency code
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// Trading product information
/// </summary>
public class CoinbaseProduct
{
    /// <summary>
    /// Product ID (e.g., "BTC-USDC")
    /// </summary>
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Current price
    /// </summary>
    [JsonPropertyName("price")]
    public string Price { get; set; } = "0";

    /// <summary>
    /// 24-hour price change percentage
    /// </summary>
    [JsonPropertyName("price_percentage_change_24h")]
    public string PricePercentageChange24h { get; set; } = "0";

    /// <summary>
    /// 24-hour volume
    /// </summary>
    [JsonPropertyName("volume_24h")]
    public string Volume24h { get; set; } = "0";

    /// <summary>
    /// 24-hour volume change percentage
    /// </summary>
    [JsonPropertyName("volume_percentage_change_24h")]
    public string VolumePercentageChange24h { get; set; } = "0";

    /// <summary>
    /// Base increment for order size
    /// </summary>
    [JsonPropertyName("base_increment")]
    public string BaseIncrement { get; set; } = "0";

    /// <summary>
    /// Quote increment for order price
    /// </summary>
    [JsonPropertyName("quote_increment")]
    public string QuoteIncrement { get; set; } = "0";

    /// <summary>
    /// Minimum order size in quote currency
    /// </summary>
    [JsonPropertyName("quote_min_size")]
    public string QuoteMinSize { get; set; } = "0";

    /// <summary>
    /// Maximum order size in quote currency
    /// </summary>
    [JsonPropertyName("quote_max_size")]
    public string QuoteMaxSize { get; set; } = "0";

    /// <summary>
    /// Minimum order size in base currency
    /// </summary>
    [JsonPropertyName("base_min_size")]
    public string BaseMinSize { get; set; } = "0";

    /// <summary>
    /// Maximum order size in base currency
    /// </summary>
    [JsonPropertyName("base_max_size")]
    public string BaseMaxSize { get; set; } = "0";

    /// <summary>
    /// Base currency name
    /// </summary>
    [JsonPropertyName("base_name")]
    public string BaseName { get; set; } = string.Empty;

    /// <summary>
    /// Quote currency name
    /// </summary>
    [JsonPropertyName("quote_name")]
    public string QuoteName { get; set; } = string.Empty;

    /// <summary>
    /// Whether product is watched
    /// </summary>
    [JsonPropertyName("watched")]
    public bool Watched { get; set; }

    /// <summary>
    /// Whether product is disabled
    /// </summary>
    [JsonPropertyName("is_disabled")]
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Whether product is new
    /// </summary>
    [JsonPropertyName("new")]
    public bool New { get; set; }

    /// <summary>
    /// Product status
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Whether cancel only mode is active
    /// </summary>
    [JsonPropertyName("cancel_only")]
    public bool CancelOnly { get; set; }

    /// <summary>
    /// Whether limit only mode is active
    /// </summary>
    [JsonPropertyName("limit_only")]
    public bool LimitOnly { get; set; }

    /// <summary>
    /// Whether post only mode is active
    /// </summary>
    [JsonPropertyName("post_only")]
    public bool PostOnly { get; set; }

    /// <summary>
    /// Whether trading is disabled
    /// </summary>
    [JsonPropertyName("trading_disabled")]
    public bool TradingDisabled { get; set; }

    /// <summary>
    /// Whether auction mode is active
    /// </summary>
    [JsonPropertyName("auction_mode")]
    public bool AuctionMode { get; set; }

    /// <summary>
    /// Product type
    /// </summary>
    [JsonPropertyName("product_type")]
    public string ProductType { get; set; } = string.Empty;

    /// <summary>
    /// Quote currency ID
    /// </summary>
    [JsonPropertyName("quote_currency_id")]
    public string QuoteCurrencyId { get; set; } = string.Empty;

    /// <summary>
    /// Base currency ID
    /// </summary>
    [JsonPropertyName("base_currency_id")]
    public string BaseCurrencyId { get; set; } = string.Empty;

    /// <summary>
    /// FCM trading session details
    /// </summary>
    [JsonPropertyName("fcm_trading_session_details")]
    public object? FcmTradingSessionDetails { get; set; }

    /// <summary>
    /// Mid market price
    /// </summary>
    [JsonPropertyName("mid_market_price")]
    public string? MidMarketPrice { get; set; }
}

/// <summary>
/// Product ticker information
/// </summary>
public class CoinbaseProductTicker
{
    /// <summary>
    /// Best ask price
    /// </summary>
    [JsonPropertyName("ask")]
    public string Ask { get; set; } = "0";

    /// <summary>
    /// Best bid price
    /// </summary>
    [JsonPropertyName("bid")]
    public string Bid { get; set; } = "0";

    /// <summary>
    /// Volume traded
    /// </summary>
    [JsonPropertyName("volume")]
    public string Volume { get; set; } = "0";

    /// <summary>
    /// Trade ID
    /// </summary>
    [JsonPropertyName("trade_id")]
    public string TradeId { get; set; } = string.Empty;

    /// <summary>
    /// Last price
    /// </summary>
    [JsonPropertyName("price")]
    public string Price { get; set; } = "0";

    /// <summary>
    /// Last trade size
    /// </summary>
    [JsonPropertyName("size")]
    public string Size { get; set; } = "0";

    /// <summary>
    /// Time of last trade
    /// </summary>
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }
}

/// <summary>
/// Order fill information
/// </summary>
public class CoinbaseFill
{
    /// <summary>
    /// Fill ID
    /// </summary>
    [JsonPropertyName("entry_id")]
    public string EntryId { get; set; } = string.Empty;

    /// <summary>
    /// Trade ID
    /// </summary>
    [JsonPropertyName("trade_id")]
    public string TradeId { get; set; } = string.Empty;

    /// <summary>
    /// Order ID
    /// </summary>
    [JsonPropertyName("order_id")]
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Trade time
    /// </summary>
    [JsonPropertyName("trade_time")]
    public DateTime TradeTime { get; set; }

    /// <summary>
    /// Trade type
    /// </summary>
    [JsonPropertyName("trade_type")]
    public string TradeType { get; set; } = string.Empty;

    /// <summary>
    /// Fill price
    /// </summary>
    [JsonPropertyName("price")]
    public string Price { get; set; } = "0";

    /// <summary>
    /// Fill size
    /// </summary>
    [JsonPropertyName("size")]
    public string Size { get; set; } = "0";

    /// <summary>
    /// Commission paid
    /// </summary>
    [JsonPropertyName("commission")]
    public string Commission { get; set; } = "0";

    /// <summary>
    /// Product ID
    /// </summary>
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Sequence timestamp
    /// </summary>
    [JsonPropertyName("sequence_timestamp")]
    public DateTime SequenceTimestamp { get; set; }

    /// <summary>
    /// Liquidity indicator (MAKER or TAKER)
    /// </summary>
    [JsonPropertyName("liquidity_indicator")]
    public string LiquidityIndicator { get; set; } = string.Empty;

    /// <summary>
    /// Whether size is in quote currency
    /// </summary>
    [JsonPropertyName("size_in_quote")]
    public bool SizeInQuote { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Order side
    /// </summary>
    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;
}

/// <summary>
/// Portfolio information
/// </summary>
public class CoinbasePortfolio
{
    /// <summary>
    /// Portfolio UUID
    /// </summary>
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// Portfolio name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether portfolio is deleted
    /// </summary>
    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }
}

/// <summary>
/// Fee tier information
/// </summary>
public class CoinbaseFeeTier
{
    /// <summary>
    /// Maker fee rate
    /// </summary>
    [JsonPropertyName("maker_fee_rate")]
    public string MakerFeeRate { get; set; } = "0";

    /// <summary>
    /// Taker fee rate
    /// </summary>
    [JsonPropertyName("taker_fee_rate")]
    public string TakerFeeRate { get; set; } = "0";

    /// <summary>
    /// USD volume
    /// </summary>
    [JsonPropertyName("usd_volume")]
    public string UsdVolume { get; set; } = "0";
}

/// <summary>
/// WebSocket message
/// </summary>
public class CoinbaseWebSocketMessage
{
    /// <summary>
    /// Message channel
    /// </summary>
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Client ID
    /// </summary>
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Message timestamp
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Sequence number
    /// </summary>
    [JsonPropertyName("sequence_num")]
    public int SequenceNum { get; set; }

    /// <summary>
    /// Message events
    /// </summary>
    [JsonPropertyName("events")]
    public List<CoinbaseWebSocketEvent> Events { get; set; } = new();
}

/// <summary>
/// WebSocket event
/// </summary>
public class CoinbaseWebSocketEvent
{
    /// <summary>
    /// Event type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Product updates
    /// </summary>
    [JsonPropertyName("products")]
    public List<CoinbaseProduct>? Products { get; set; }

    /// <summary>
    /// Order updates
    /// </summary>
    [JsonPropertyName("orders")]
    public List<CoinbaseOrderUpdate>? Orders { get; set; }
}

/// <summary>
/// Order update from WebSocket
/// </summary>
public class CoinbaseOrderUpdate
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
    public string ClientOrderId { get; set; } = string.Empty;

    /// <summary>
    /// Cumulative quantity filled
    /// </summary>
    [JsonPropertyName("cumulative_quantity")]
    public string CumulativeQuantity { get; set; } = "0";

    /// <summary>
    /// Remaining quantity
    /// </summary>
    [JsonPropertyName("leaves_quantity")]
    public string LeavesQuantity { get; set; } = "0";

    /// <summary>
    /// Average price
    /// </summary>
    [JsonPropertyName("avg_price")]
    public string AvgPrice { get; set; } = "0";

    /// <summary>
    /// Total fees
    /// </summary>
    [JsonPropertyName("total_fees")]
    public string TotalFees { get; set; } = "0";

    /// <summary>
    /// Order status
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Product ID
    /// </summary>
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Creation time
    /// </summary>
    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Order side
    /// </summary>
    [JsonPropertyName("order_side")]
    public string OrderSide { get; set; } = string.Empty;

    /// <summary>
    /// Order type
    /// </summary>
    [JsonPropertyName("order_type")]
    public string OrderType { get; set; } = string.Empty;
}

/// <summary>
/// Candle/OHLC data
/// </summary>
public class CoinbaseCandle
{
    /// <summary>
    /// Start time
    /// </summary>
    [JsonPropertyName("start")]
    public string Start { get; set; } = string.Empty;

    /// <summary>
    /// Low price
    /// </summary>
    [JsonPropertyName("low")]
    public string Low { get; set; } = "0";

    /// <summary>
    /// High price
    /// </summary>
    [JsonPropertyName("high")]
    public string High { get; set; } = "0";

    /// <summary>
    /// Open price
    /// </summary>
    [JsonPropertyName("open")]
    public string Open { get; set; } = "0";

    /// <summary>
    /// Close price
    /// </summary>
    [JsonPropertyName("close")]
    public string Close { get; set; } = "0";

    /// <summary>
    /// Volume
    /// </summary>
    [JsonPropertyName("volume")]
    public string Volume { get; set; } = "0";
}

/// <summary>
/// Order book data
/// </summary>
public class CoinbaseOrderBook
{
    /// <summary>
    /// Product ID
    /// </summary>
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Bid orders
    /// </summary>
    [JsonPropertyName("bids")]
    public List<CoinbaseOrderBookEntry> Bids { get; set; } = new();

    /// <summary>
    /// Ask orders
    /// </summary>
    [JsonPropertyName("asks")]
    public List<CoinbaseOrderBookEntry> Asks { get; set; } = new();

    /// <summary>
    /// Order book time
    /// </summary>
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }
}

/// <summary>
/// Order book entry
/// </summary>
public class CoinbaseOrderBookEntry
{
    /// <summary>
    /// Price level
    /// </summary>
    [JsonPropertyName("price")]
    public string Price { get; set; } = "0";

    /// <summary>
    /// Size at price level
    /// </summary>
    [JsonPropertyName("size")]
    public string Size { get; set; } = "0";
}

/// <summary>
/// API error response
/// </summary>
public class CoinbaseErrorResponse
{
    /// <summary>
    /// Error code
    /// </summary>
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error details
    /// </summary>
    [JsonPropertyName("error_details")]
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Preview failure reason
    /// </summary>
    [JsonPropertyName("preview_failure_reason")]
    public string? PreviewFailureReason { get; set; }

    /// <summary>
    /// New order failure reason
    /// </summary>
    [JsonPropertyName("new_order_failure_reason")]
    public string? NewOrderFailureReason { get; set; }
}