using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PaymentGatewayService.Models.Coinbase;

namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// Implementation of Coinbase Advanced Trade API service
/// </summary>
public class CoinbaseService : ICoinbaseService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CoinbaseService> _logger;
    private readonly ICoinbaseSignatureService _signatureService;
    private readonly IWebSocketService _webSocketService;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _webSocketUrl;
    private readonly Dictionary<string, string> _activeSubscriptions;

    public CoinbaseService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CoinbaseService> logger,
        ICoinbaseSignatureService signatureService,
        IWebSocketService webSocketService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _signatureService = signatureService;
        _webSocketService = webSocketService;
        _apiKey = configuration["Coinbase:ApiKey"] ?? string.Empty;
        _baseUrl = configuration["Coinbase:BaseUrl"] ?? "https://api.coinbase.com/api/v3/brokerage/";
        _webSocketUrl = configuration["Coinbase:WebSocketUrl"] ?? "wss://advanced-trade-ws.coinbase.com";
        _activeSubscriptions = new Dictionary<string, string>();
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };

        // Set base URL if not already set
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }
    }

    /// <summary>
    /// Places a new order on Coinbase
    /// </summary>
    public async Task<CoinbaseOrderResponse> CreateOrderAsync(CoinbaseOrderRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating Coinbase order. CorrelationId: {CorrelationId}, ProductId: {ProductId}, Side: {Side}", 
            correlationId, request.ProductId, request.Side);

        try
        {
            var response = await SendAuthenticatedRequestAsync<CoinbaseOrderResponse>(
                HttpMethod.Post,
                "orders",
                request);

            _logger.LogInformation("Coinbase order created successfully. CorrelationId: {CorrelationId}, OrderId: {OrderId}, Status: {Status}", 
                correlationId, response.OrderId, response.Status);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Coinbase order. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Cancels an existing order
    /// </summary>
    public async Task<List<string>> CancelOrderAsync(string orderId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Cancelling Coinbase order. CorrelationId: {CorrelationId}, OrderId: {OrderId}", 
            correlationId, orderId);

        try
        {
            var requestBody = new { order_ids = new[] { orderId } };
            var response = await SendAuthenticatedRequestAsync<CancelOrdersResponse>(
                HttpMethod.Post,
                "orders/batch_cancel",
                requestBody);

            _logger.LogInformation("Coinbase order cancelled. CorrelationId: {CorrelationId}, OrderId: {OrderId}", 
                correlationId, orderId);

            return response.OrderIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling Coinbase order. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets the status of an order
    /// </summary>
    public async Task<CoinbaseOrderResponse> GetOrderAsync(string orderId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting Coinbase order. CorrelationId: {CorrelationId}, OrderId: {OrderId}", 
            correlationId, orderId);

        try
        {
            var response = await SendAuthenticatedRequestAsync<CoinbaseOrderResponse>(
                HttpMethod.Get,
                $"orders/historical/{orderId}",
                null);

            _logger.LogInformation("Retrieved Coinbase order. CorrelationId: {CorrelationId}, Status: {Status}", 
                correlationId, response.Status);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Coinbase order. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Lists all orders with optional filtering
    /// </summary>
    public async Task<List<CoinbaseOrderResponse>> ListOrdersAsync(string? productId = null, string? status = null, int limit = 100)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Listing Coinbase orders. CorrelationId: {CorrelationId}, ProductId: {ProductId}, Status: {Status}", 
            correlationId, productId, status);

        try
        {
            var queryParams = new List<string> { $"limit={limit}" };
            if (!string.IsNullOrEmpty(productId))
                queryParams.Add($"product_id={productId}");
            if (!string.IsNullOrEmpty(status))
                queryParams.Add($"order_status={status}");

            var query = string.Join("&", queryParams);
            var response = await SendAuthenticatedRequestAsync<OrdersListResponse>(
                HttpMethod.Get,
                $"orders/historical/batch?{query}",
                null);

            _logger.LogInformation("Retrieved {Count} Coinbase orders. CorrelationId: {CorrelationId}", 
                response.Orders.Count, correlationId);

            return response.Orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Coinbase orders. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets account information and balances
    /// </summary>
    public async Task<List<CoinbaseAccount>> GetAccountsAsync(string? accountId = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting Coinbase accounts. CorrelationId: {CorrelationId}", correlationId);

        try
        {
            string endpoint = "accounts";
            if (!string.IsNullOrEmpty(accountId))
                endpoint = $"accounts/{accountId}";

            var response = await SendAuthenticatedRequestAsync<AccountsResponse>(
                HttpMethod.Get,
                endpoint,
                null);

            _logger.LogInformation("Retrieved {Count} Coinbase accounts. CorrelationId: {CorrelationId}", 
                response.Accounts.Count, correlationId);

            return response.Accounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Coinbase accounts. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets available trading products/pairs
    /// </summary>
    public async Task<List<CoinbaseProduct>> GetProductsAsync(string? productId = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting Coinbase products. CorrelationId: {CorrelationId}", correlationId);

        try
        {
            string endpoint = "products";
            if (!string.IsNullOrEmpty(productId))
                endpoint = $"products/{productId}";

            var response = await SendAuthenticatedRequestAsync<ProductsResponse>(
                HttpMethod.Get,
                endpoint,
                null);

            _logger.LogInformation("Retrieved {Count} Coinbase products. CorrelationId: {CorrelationId}", 
                response.Products.Count, correlationId);

            return response.Products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Coinbase products. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets the current best bid/ask for a product
    /// </summary>
    public async Task<CoinbaseProductTicker> GetProductTickerAsync(string productId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting product ticker. CorrelationId: {CorrelationId}, ProductId: {ProductId}", 
            correlationId, productId);

        try
        {
            var response = await SendAuthenticatedRequestAsync<TickerResponse>(
                HttpMethod.Get,
                $"products/{productId}/ticker",
                null);

            _logger.LogInformation("Retrieved ticker for {ProductId}. Bid: {Bid}, Ask: {Ask}", 
                productId, response.Ticker.Bid, response.Ticker.Ask);

            return response.Ticker;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product ticker. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Creates a new portfolio for isolated trading
    /// </summary>
    public async Task<CoinbasePortfolio> CreatePortfolioAsync(string name)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating portfolio. CorrelationId: {CorrelationId}, Name: {Name}", 
            correlationId, name);

        try
        {
            var request = new CoinbaseCreatePortfolioRequest { Name = name };
            var response = await SendAuthenticatedRequestAsync<CoinbasePortfolio>(
                HttpMethod.Post,
                "portfolios",
                request);

            _logger.LogInformation("Portfolio created successfully. CorrelationId: {CorrelationId}, PortfolioId: {PortfolioId}", 
                correlationId, response.Uuid);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating portfolio. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Lists all portfolios
    /// </summary>
    public async Task<List<CoinbasePortfolio>> ListPortfoliosAsync()
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Listing portfolios. CorrelationId: {CorrelationId}", correlationId);

        try
        {
            var response = await SendAuthenticatedRequestAsync<PortfoliosResponse>(
                HttpMethod.Get,
                "portfolios",
                null);

            _logger.LogInformation("Retrieved {Count} portfolios. CorrelationId: {CorrelationId}", 
                response.Portfolios.Count, correlationId);

            return response.Portfolios;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing portfolios. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Validates connection and authentication
    /// </summary>
    public async Task<bool> ValidateConnectionAsync()
    {
        try
        {
            // Try to get accounts as a validation check
            var accounts = await GetAccountsAsync();
            return accounts.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection validation failed");
            return false;
        }
    }

    /// <summary>
    /// Gets order fills for a specific order
    /// </summary>
    public async Task<List<CoinbaseFill>> GetOrderFillsAsync(string orderId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting order fills. CorrelationId: {CorrelationId}, OrderId: {OrderId}", 
            correlationId, orderId);

        try
        {
            var response = await SendAuthenticatedRequestAsync<FillsResponse>(
                HttpMethod.Get,
                $"orders/historical/fills?order_id={orderId}",
                null);

            _logger.LogInformation("Retrieved {Count} fills for order {OrderId}", 
                response.Fills.Count, orderId);

            return response.Fills;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order fills. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets trading fees for the authenticated user
    /// </summary>
    public async Task<CoinbaseFeeTier> GetFeeTierAsync()
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting fee tier. CorrelationId: {CorrelationId}", correlationId);

        try
        {
            var response = await SendAuthenticatedRequestAsync<FeeTierResponse>(
                HttpMethod.Get,
                "transaction_summary",
                null);

            _logger.LogInformation("Retrieved fee tier. Maker: {Maker}, Taker: {Taker}", 
                response.FeeTier.MakerFeeRate, response.FeeTier.TakerFeeRate);

            return response.FeeTier;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fee tier. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Subscribes to WebSocket channels for real-time updates
    /// </summary>
    public async Task<string> SubscribeToWebSocketAsync(
        List<string> channels, 
        List<string> productIds, 
        Action<CoinbaseWebSocketMessage> messageHandler)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Subscribing to WebSocket. CorrelationId: {CorrelationId}, Channels: {Channels}", 
            correlationId, string.Join(",", channels));

        try
        {
            var jwt = _signatureService.GenerateWebSocketJWT();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var subscribeRequest = new CoinbaseWebSocketSubscribeRequest
            {
                Type = "subscribe",
                ProductIds = productIds,
                Channel = channels.First(), // Coinbase supports one channel per subscription
                JWT = jwt,
                Timestamp = timestamp
            };

            var subscribeJson = JsonSerializer.Serialize(subscribeRequest, _jsonOptions);

            // Create WebSocket connection
            var connectionId = await _webSocketService.ConnectAsync(
                new Uri(_webSocketUrl),
                (message) =>
                {
                    try
                    {
                        var wsMessage = JsonSerializer.Deserialize<CoinbaseWebSocketMessage>(message, _jsonOptions);
                        if (wsMessage != null)
                        {
                            messageHandler(wsMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing WebSocket message");
                    }
                });

            // Send subscription request
            await _webSocketService.SendMessageAsync(connectionId, subscribeJson);

            _activeSubscriptions[connectionId] = string.Join(",", channels);
            
            _logger.LogInformation("WebSocket subscription established. CorrelationId: {CorrelationId}, ConnectionId: {ConnectionId}", 
                correlationId, connectionId);

            return connectionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to WebSocket. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Unsubscribes from WebSocket channels
    /// </summary>
    public async Task<bool> UnsubscribeFromWebSocketAsync(string subscriptionId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Unsubscribing from WebSocket. CorrelationId: {CorrelationId}, SubscriptionId: {SubscriptionId}", 
            correlationId, subscriptionId);

        try
        {
            if (_activeSubscriptions.ContainsKey(subscriptionId))
            {
                await _webSocketService.DisconnectAsync(subscriptionId);
                _activeSubscriptions.Remove(subscriptionId);
                
                _logger.LogInformation("WebSocket unsubscribed successfully. CorrelationId: {CorrelationId}", correlationId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from WebSocket. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets historical candle data for a product
    /// </summary>
    public async Task<List<CoinbaseCandle>> GetCandlesAsync(
        string productId, 
        int granularity, 
        DateTime start, 
        DateTime end)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting candles. CorrelationId: {CorrelationId}, ProductId: {ProductId}, Granularity: {Granularity}", 
            correlationId, productId, granularity);

        try
        {
            var startUnix = new DateTimeOffset(start).ToUnixTimeSeconds();
            var endUnix = new DateTimeOffset(end).ToUnixTimeSeconds();

            var response = await SendAuthenticatedRequestAsync<CandlesResponse>(
                HttpMethod.Get,
                $"products/{productId}/candles?start={startUnix}&end={endUnix}&granularity={granularity}",
                null);

            _logger.LogInformation("Retrieved {Count} candles for {ProductId}", 
                response.Candles.Count, productId);

            return response.Candles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candles. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets the order book for a product
    /// </summary>
    public async Task<CoinbaseOrderBook> GetOrderBookAsync(string productId, int level = 2)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting order book. CorrelationId: {CorrelationId}, ProductId: {ProductId}, Level: {Level}", 
            correlationId, productId, level);

        try
        {
            var response = await SendAuthenticatedRequestAsync<OrderBookResponse>(
                HttpMethod.Get,
                $"products/{productId}/book?level={level}",
                null);

            _logger.LogInformation("Retrieved order book for {ProductId}. Bids: {BidCount}, Asks: {AskCount}", 
                productId, response.OrderBook.Bids.Count, response.OrderBook.Asks.Count);

            return response.OrderBook;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order book. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// Sends an authenticated request to Coinbase API
    /// </summary>
    private async Task<T> SendAuthenticatedRequestAsync<T>(HttpMethod method, string path, object? body)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var requestPath = $"/api/v3/brokerage/{path}";
        var bodyString = body != null ? JsonSerializer.Serialize(body, _jsonOptions) : string.Empty;

        // Generate signature
        var signature = _signatureService.GenerateSignature(
            method.Method,
            requestPath,
            bodyString,
            timestamp);

        // Create request
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("CB-ACCESS-KEY", _apiKey);
        request.Headers.Add("CB-ACCESS-SIGN", signature);
        request.Headers.Add("CB-ACCESS-TIMESTAMP", timestamp);
        request.Headers.Add("CB-VERSION", "2023-01-01");

        if (body != null)
        {
            request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
        }

        // Send request
        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = $"HTTP {response.StatusCode}: {responseContent}";
            
            _logger.LogError("Coinbase API error: {StatusCode} - {Message}", response.StatusCode, errorMessage);
            throw new HttpRequestException($"Coinbase API error: {errorMessage}");
        }

        var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    #endregion

    #region Response Wrapper Classes

    private class OrdersListResponse
    {
        [JsonPropertyName("orders")]
        public List<CoinbaseOrderResponse> Orders { get; set; } = new();

        [JsonPropertyName("sequence")]
        public long Sequence { get; set; }

        [JsonPropertyName("has_next")]
        public bool HasNext { get; set; }

        [JsonPropertyName("cursor")]
        public string? Cursor { get; set; }
    }

    private class AccountsResponse
    {
        [JsonPropertyName("accounts")]
        public List<CoinbaseAccount> Accounts { get; set; } = new();

        [JsonPropertyName("has_next")]
        public bool HasNext { get; set; }

        [JsonPropertyName("cursor")]
        public string? Cursor { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }
    }

    private class ProductsResponse
    {
        [JsonPropertyName("products")]
        public List<CoinbaseProduct> Products { get; set; } = new();

        [JsonPropertyName("num_products")]
        public int NumProducts { get; set; }
    }

    private class PortfoliosResponse
    {
        [JsonPropertyName("portfolios")]
        public List<CoinbasePortfolio> Portfolios { get; set; } = new();
    }

    private class FillsResponse
    {
        [JsonPropertyName("fills")]
        public List<CoinbaseFill> Fills { get; set; } = new();

        [JsonPropertyName("cursor")]
        public string? Cursor { get; set; }
    }

    private class FeeTierResponse
    {
        [JsonPropertyName("fee_tier")]
        public CoinbaseFeeTier FeeTier { get; set; } = new();

        [JsonPropertyName("margin_rate")]
        public object? MarginRate { get; set; }

        [JsonPropertyName("goods_and_services_tax")]
        public object? GoodsAndServicesTax { get; set; }
    }

    private class CandlesResponse
    {
        [JsonPropertyName("candles")]
        public List<CoinbaseCandle> Candles { get; set; } = new();
    }

    private class OrderBookResponse
    {
        [JsonPropertyName("pricebook")]
        public CoinbaseOrderBook OrderBook { get; set; } = new();
    }

    private class TickerResponse
    {
        [JsonPropertyName("ticker")]
        public CoinbaseProductTicker Ticker { get; set; } = new();
    }

    private class CancelOrdersResponse
    {
        [JsonPropertyName("results")]
        public List<CancelOrderResult> Results { get; set; } = new();

        public List<string> OrderIds => Results.Where(r => r.Success).Select(r => r.OrderId).ToList();
    }

    private class CancelOrderResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("failure_reason")]
        public string? FailureReason { get; set; }

        [JsonPropertyName("order_id")]
        public string OrderId { get; set; } = string.Empty;
    }

    #endregion
}
