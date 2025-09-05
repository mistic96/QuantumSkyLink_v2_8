using PaymentGatewayService.Models.Coinbase;

namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// Interface for Coinbase Advanced Trade API integration
/// </summary>
public interface ICoinbaseService
{
    /// <summary>
    /// Places a new order on Coinbase
    /// </summary>
    /// <param name="request">Order request details</param>
    /// <returns>Order response with transaction details</returns>
    Task<CoinbaseOrderResponse> CreateOrderAsync(CoinbaseOrderRequest request);

    /// <summary>
    /// Cancels an existing order
    /// </summary>
    /// <param name="orderId">The order ID to cancel</param>
    /// <returns>List of cancelled order IDs</returns>
    Task<List<string>> CancelOrderAsync(string orderId);

    /// <summary>
    /// Gets the status of an order
    /// </summary>
    /// <param name="orderId">The order ID to check</param>
    /// <returns>Order details with current status</returns>
    Task<CoinbaseOrderResponse> GetOrderAsync(string orderId);

    /// <summary>
    /// Lists all orders with optional filtering
    /// </summary>
    /// <param name="productId">Optional product ID filter</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="limit">Maximum number of orders to return</param>
    /// <returns>List of orders</returns>
    Task<List<CoinbaseOrderResponse>> ListOrdersAsync(string? productId = null, string? status = null, int limit = 100);

    /// <summary>
    /// Gets account information and balances
    /// </summary>
    /// <param name="accountId">Optional specific account ID</param>
    /// <returns>List of account details with balances</returns>
    Task<List<CoinbaseAccount>> GetAccountsAsync(string? accountId = null);

    /// <summary>
    /// Gets available trading products/pairs
    /// </summary>
    /// <param name="productId">Optional specific product ID</param>
    /// <returns>List of available trading products</returns>
    Task<List<CoinbaseProduct>> GetProductsAsync(string? productId = null);

    /// <summary>
    /// Gets the current best bid/ask for a product
    /// </summary>
    /// <param name="productId">The product ID (e.g., "BTC-USDC")</param>
    /// <returns>Product ticker information</returns>
    Task<CoinbaseProductTicker> GetProductTickerAsync(string productId);

    /// <summary>
    /// Creates a new portfolio for isolated trading
    /// </summary>
    /// <param name="name">Portfolio name</param>
    /// <returns>Created portfolio details</returns>
    Task<CoinbasePortfolio> CreatePortfolioAsync(string name);

    /// <summary>
    /// Lists all portfolios
    /// </summary>
    /// <returns>List of portfolios</returns>
    Task<List<CoinbasePortfolio>> ListPortfoliosAsync();

    /// <summary>
    /// Validates connection and authentication
    /// </summary>
    /// <returns>True if connection is valid</returns>
    Task<bool> ValidateConnectionAsync();

    /// <summary>
    /// Gets order fills for a specific order
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>List of fills for the order</returns>
    Task<List<CoinbaseFill>> GetOrderFillsAsync(string orderId);

    /// <summary>
    /// Gets trading fees for the authenticated user
    /// </summary>
    /// <returns>Fee tier information</returns>
    Task<CoinbaseFeeTier> GetFeeTierAsync();

    /// <summary>
    /// Subscribes to WebSocket channels for real-time updates
    /// </summary>
    /// <param name="channels">List of channels to subscribe to</param>
    /// <param name="productIds">List of product IDs to monitor</param>
    /// <param name="messageHandler">Handler for incoming messages</param>
    /// <returns>Subscription ID for management</returns>
    Task<string> SubscribeToWebSocketAsync(
        List<string> channels, 
        List<string> productIds, 
        Action<CoinbaseWebSocketMessage> messageHandler);

    /// <summary>
    /// Unsubscribes from WebSocket channels
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to cancel</param>
    /// <returns>True if successfully unsubscribed</returns>
    Task<bool> UnsubscribeFromWebSocketAsync(string subscriptionId);

    /// <summary>
    /// Gets historical candle data for a product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="granularity">Time granularity in seconds (60, 300, 900, 3600, 21600, 86400)</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <returns>List of candle data</returns>
    Task<List<CoinbaseCandle>> GetCandlesAsync(
        string productId, 
        int granularity, 
        DateTime start, 
        DateTime end);

    /// <summary>
    /// Gets the order book for a product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="level">Order book level (1=best bid/ask, 2=top 50, 3=full)</param>
    /// <returns>Order book data</returns>
    Task<CoinbaseOrderBook> GetOrderBookAsync(string productId, int level = 2);
}