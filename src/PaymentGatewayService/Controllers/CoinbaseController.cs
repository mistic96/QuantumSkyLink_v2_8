using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.Coinbase;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Services.Integrations;
using PaymentGatewayService.Services.Interfaces;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PaymentGatewayService.Utils;

namespace PaymentGatewayService.Controllers;

/// <summary>
/// Controller for Coinbase Advanced Trade API operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoinbaseController : ControllerBase
{
    private readonly ICoinbaseService _coinbaseService;
    private readonly IPaymentService _paymentService;
    private readonly PaymentDbContext _context;
    private readonly ILogger<CoinbaseController> _logger;

    public CoinbaseController(
        ICoinbaseService coinbaseService,
        IPaymentService paymentService,
        PaymentDbContext context,
        ILogger<CoinbaseController> logger)
    {
        _coinbaseService = coinbaseService;
        _paymentService = paymentService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new trading order
    /// </summary>
    /// <param name="request">Order details</param>
    /// <returns>Order response with ID and status</returns>
    [HttpPost("orders")]
    [ProducesResponseType(typeof(CoinbaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CoinbaseOrderResponse>> CreateOrder([FromBody] CreateCoinbaseOrderRequest request)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("User ID not found or invalid in token");
            }

            _logger.LogInformation("Creating Coinbase order for user {UserId}, ProductId: {ProductId}, Side: {Side}, Type: {Type}", 
                userId, request.ProductId, request.Side, request.OrderType);

            // Find Coinbase gateway
            var gateway = await _context.PaymentGateways
                .FirstOrDefaultAsync(g => g.GatewayType == PaymentGatewayType.Coinbase && g.IsActive);

            if (gateway == null)
            {
                return BadRequest("Coinbase gateway is not configured or active");
            }

            // Create payment record for tracking
            var payment = new Payment
            {
                UserId = userId,
                PaymentGatewayId = gateway.Id,
                PaymentGateway = gateway.GatewayType,
                Amount = request.OrderType == "market" && request.Side == "buy" ? request.QuoteSize ?? 0 : request.BaseSize ?? 0,
                Currency = request.Side == "buy" ? request.ProductId.Split('-')[1] : request.ProductId.Split('-')[0],
                Type = request.Side == "buy" ? PaymentType.Deposit : PaymentType.Withdrawal,
                Status = PaymentStatus.Pending,
                Metadata = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["productId"] = request.ProductId,
                    ["orderType"] = request.OrderType,
                    ["side"] = request.Side,
                    ["limitPrice"] = request.LimitPrice?.ToString() ?? "",
                    ["stopPrice"] = request.StopPrice?.ToString() ?? "",
                    ["timeInForce"] = request.TimeInForce ?? "gtc"
                })
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Prepare Coinbase order request
            var coinbaseRequest = new CoinbaseOrderRequest
            {
                ClientOrderId = payment.Id.ToString(),
                ProductId = request.ProductId,
                Side = request.Side,
                OrderConfiguration = BuildOrderConfiguration(request)
            };

            // Create order through Coinbase
            var result = await _coinbaseService.CreateOrderAsync(coinbaseRequest);

            // Update payment with Coinbase response
            payment.GatewayTransactionId = result.OrderId;
            payment.Status = result.Status.ToLower() switch
            {
                "filled" => PaymentStatus.Completed,
                "pending" => PaymentStatus.Pending,
                "cancelled" => PaymentStatus.Cancelled,
                "failed" => PaymentStatus.Failed,
                _ => PaymentStatus.Processing
            };

            // Store additional details in metadata
            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new();
            metadata["coinbaseOrderId"] = result.OrderId;
            metadata["averageFilledPrice"] = result.AverageFilledPrice;
            metadata["filledSize"] = result.FilledSize;
            metadata["fee"] = result.Fee;
            payment.Metadata = JsonSerializer.Serialize(metadata);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Coinbase order created. PaymentId: {PaymentId}, OrderId: {OrderId}, Status: {Status}", 
                payment.Id, result.OrderId, result.Status);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Coinbase order");
            return StatusCode(500, new { error = "Failed to create order", message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel an existing order
    /// </summary>
    /// <param name="orderId">Order ID to cancel</param>
    /// <returns>List of cancelled order IDs</returns>
    [HttpDelete("orders/{orderId}")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<string>>> CancelOrder(string orderId)
    {
        try
        {
            _logger.LogInformation("Cancelling Coinbase order {OrderId}", orderId);

            var result = await _coinbaseService.CancelOrderAsync(orderId);

            // Update payment status if found
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.GatewayTransactionId == orderId);
            
            if (payment != null)
            {
                payment.Status = PaymentStatus.Cancelled;
                await _context.SaveChangesAsync();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling Coinbase order");
            return StatusCode(500, new { error = "Failed to cancel order", message = ex.Message });
        }
    }

    /// <summary>
    /// Get order details
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("orders/{orderId}")]
    [ProducesResponseType(typeof(CoinbaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoinbaseOrderResponse>> GetOrder(string orderId)
    {
        try
        {
            _logger.LogInformation("Getting Coinbase order {OrderId}", orderId);

            var result = await _coinbaseService.GetOrderAsync(orderId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Coinbase order");
            return StatusCode(500, new { error = "Failed to get order", message = ex.Message });
        }
    }

    /// <summary>
    /// List orders with filtering
    /// </summary>
    /// <param name="productId">Filter by product ID</param>
    /// <param name="status">Filter by status</param>
    /// <param name="limit">Maximum number of orders to return</param>
    /// <returns>List of orders</returns>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(List<CoinbaseOrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CoinbaseOrderResponse>>> ListOrders(
        [FromQuery] string? productId = null,
        [FromQuery] string? status = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            _logger.LogInformation("Listing Coinbase orders. ProductId: {ProductId}, Status: {Status}, Limit: {Limit}", 
                productId, status, limit);

            var result = await _coinbaseService.ListOrdersAsync(productId, status, limit);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Coinbase orders");
            return StatusCode(500, new { error = "Failed to list orders", message = ex.Message });
        }
    }

    /// <summary>
    /// Get account balances
    /// </summary>
    /// <param name="accountId">Optional account ID filter</param>
    /// <returns>List of accounts with balances</returns>
    [HttpGet("accounts")]
    [ProducesResponseType(typeof(List<CoinbaseAccount>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CoinbaseAccount>>> GetAccounts([FromQuery] string? accountId = null)
    {
        try
        {
            _logger.LogInformation("Getting Coinbase accounts. AccountId: {AccountId}", accountId);

            var result = await _coinbaseService.GetAccountsAsync(accountId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Coinbase accounts");
            return StatusCode(500, new { error = "Failed to get accounts", message = ex.Message });
        }
    }

    /// <summary>
    /// Get available trading products
    /// </summary>
    /// <param name="productId">Optional product ID filter</param>
    /// <returns>List of trading products</returns>
    [HttpGet("products")]
    [ProducesResponseType(typeof(List<CoinbaseProduct>), StatusCodes.Status200OK)]
    [AllowAnonymous] // Public endpoint
    public async Task<ActionResult<List<CoinbaseProduct>>> GetProducts([FromQuery] string? productId = null)
    {
        try
        {
            _logger.LogInformation("Getting Coinbase products. ProductId: {ProductId}", productId);

            var result = await _coinbaseService.GetProductsAsync(productId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Coinbase products");
            return StatusCode(500, new { error = "Failed to get products", message = ex.Message });
        }
    }

    /// <summary>
    /// Get product ticker
    /// </summary>
    /// <param name="productId">Product ID (e.g., "BTC-USDC")</param>
    /// <returns>Current ticker information</returns>
    [HttpGet("products/{productId}/ticker")]
    [ProducesResponseType(typeof(CoinbaseProductTicker), StatusCodes.Status200OK)]
    [AllowAnonymous] // Public endpoint
    public async Task<ActionResult<CoinbaseProductTicker>> GetProductTicker(string productId)
    {
        try
        {
            _logger.LogInformation("Getting ticker for product {ProductId}", productId);

            var result = await _coinbaseService.GetProductTickerAsync(productId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product ticker");
            return StatusCode(500, new { error = "Failed to get ticker", message = ex.Message });
        }
    }

    /// <summary>
    /// Get order book
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="level">Order book level (1 or 2)</param>
    /// <returns>Order book data</returns>
    [HttpGet("products/{productId}/book")]
    [ProducesResponseType(typeof(CoinbaseOrderBook), StatusCodes.Status200OK)]
    [AllowAnonymous] // Public endpoint
    public async Task<ActionResult<CoinbaseOrderBook>> GetOrderBook(string productId, [FromQuery] int level = 2)
    {
        try
        {
            _logger.LogInformation("Getting order book for product {ProductId}, level {Level}", productId, level);

            var result = await _coinbaseService.GetOrderBookAsync(productId, level);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order book");
            return StatusCode(500, new { error = "Failed to get order book", message = ex.Message });
        }
    }

    /// <summary>
    /// Get historical candles
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="granularity">Candle granularity in seconds (60, 300, 900, 3600, 21600, 86400)</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    /// <returns>Historical candle data</returns>
    [HttpGet("products/{productId}/candles")]
    [ProducesResponseType(typeof(List<CoinbaseCandle>), StatusCodes.Status200OK)]
    [AllowAnonymous] // Public endpoint
    public async Task<ActionResult<List<CoinbaseCandle>>> GetCandles(
        string productId,
        [FromQuery] int granularity = 3600,
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? end = null)
    {
        try
        {
            var startTime = start ?? DateTime.UtcNow.AddDays(-1);
            var endTime = end ?? DateTime.UtcNow;

            _logger.LogInformation("Getting candles for product {ProductId}, granularity {Granularity}", productId, granularity);

            var result = await _coinbaseService.GetCandlesAsync(productId, granularity, startTime, endTime);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candles");
            return StatusCode(500, new { error = "Failed to get candles", message = ex.Message });
        }
    }

    /// <summary>
    /// Create portfolio
    /// </summary>
    /// <param name="request">Portfolio details</param>
    /// <returns>Created portfolio</returns>
    [HttpPost("portfolios")]
    [ProducesResponseType(typeof(CoinbasePortfolio), StatusCodes.Status200OK)]
    public async Task<ActionResult<CoinbasePortfolio>> CreatePortfolio([FromBody] CreatePortfolioRequest request)
    {
        try
        {
            _logger.LogInformation("Creating portfolio with name {Name}", request.Name);

            var result = await _coinbaseService.CreatePortfolioAsync(request.Name);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating portfolio");
            return StatusCode(500, new { error = "Failed to create portfolio", message = ex.Message });
        }
    }

    /// <summary>
    /// List portfolios
    /// </summary>
    /// <returns>List of portfolios</returns>
    [HttpGet("portfolios")]
    [ProducesResponseType(typeof(List<CoinbasePortfolio>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CoinbasePortfolio>>> ListPortfolios()
    {
        try
        {
            _logger.LogInformation("Listing portfolios");

            var result = await _coinbaseService.ListPortfoliosAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing portfolios");
            return StatusCode(500, new { error = "Failed to list portfolios", message = ex.Message });
        }
    }

    /// <summary>
    /// Get order fills
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>List of fills for the order</returns>
    [HttpGet("orders/{orderId}/fills")]
    [ProducesResponseType(typeof(List<CoinbaseFill>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CoinbaseFill>>> GetOrderFills(string orderId)
    {
        try
        {
            _logger.LogInformation("Getting fills for order {OrderId}", orderId);

            var result = await _coinbaseService.GetOrderFillsAsync(orderId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order fills");
            return StatusCode(500, new { error = "Failed to get fills", message = ex.Message });
        }
    }

    /// <summary>
    /// Get fee tier
    /// </summary>
    /// <returns>Current fee tier information</returns>
    [HttpGet("fees")]
    [ProducesResponseType(typeof(CoinbaseFeeTier), StatusCodes.Status200OK)]
    public async Task<ActionResult<CoinbaseFeeTier>> GetFeeTier()
    {
        try
        {
            _logger.LogInformation("Getting fee tier");

            var result = await _coinbaseService.GetFeeTierAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fee tier");
            return StatusCode(500, new { error = "Failed to get fee tier", message = ex.Message });
        }
    }

    /// <summary>
    /// Subscribe to WebSocket updates
    /// </summary>
    /// <param name="request">WebSocket subscription request</param>
    /// <returns>Subscription ID</returns>
    [HttpPost("websocket/subscribe")]
    [ProducesResponseType(typeof(WebSocketSubscriptionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WebSocketSubscriptionResponse>> SubscribeToWebSocket([FromBody] WebSocketSubscriptionRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User {UserId} subscribing to WebSocket channels: {Channels}", 
                userId, string.Join(",", request.Channels));

            // Store subscription details for user
            var subscriptionId = await _coinbaseService.SubscribeToWebSocketAsync(
                request.Channels,
                request.ProductIds,
                (message) =>
                {
                    // In a real implementation, you would route these messages to the user
                    // through SignalR or another real-time communication mechanism
                    _logger.LogDebug("WebSocket message received for user {UserId}: {Channel}", 
                        userId, message.Channel);
                });

            return Ok(new WebSocketSubscriptionResponse 
            { 
                SubscriptionId = subscriptionId,
                Channels = request.Channels,
                ProductIds = request.ProductIds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to WebSocket");
            return StatusCode(500, new { error = "Failed to subscribe", message = ex.Message });
        }
    }

    /// <summary>
    /// Unsubscribe from WebSocket updates
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("websocket/subscribe/{subscriptionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnsubscribeFromWebSocket(string subscriptionId)
    {
        try
        {
            _logger.LogInformation("Unsubscribing from WebSocket {SubscriptionId}", subscriptionId);

            var result = await _coinbaseService.UnsubscribeFromWebSocketAsync(subscriptionId);

            if (!result)
            {
                return NotFound(new { error = "Subscription not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from WebSocket");
            return StatusCode(500, new { error = "Failed to unsubscribe", message = ex.Message });
        }
    }

    /// <summary>
    /// Validate Coinbase connection
    /// </summary>
    /// <returns>Connection status</returns>
    [HttpGet("validate")]
    [ProducesResponseType(typeof(ValidationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidationResponse>> ValidateConnection()
    {
        try
        {
            _logger.LogInformation("Validating Coinbase connection");

            var isValid = await _coinbaseService.ValidateConnectionAsync();

            return Ok(new ValidationResponse 
            { 
                IsValid = isValid,
                Message = isValid ? "Connection successful" : "Connection failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating connection");
            return Ok(new ValidationResponse 
            { 
                IsValid = false,
                Message = ex.Message
            });
        }
    }

    #region Private Methods

    private CoinbaseOrderConfiguration BuildOrderConfiguration(CreateCoinbaseOrderRequest request)
    {
        var config = new CoinbaseOrderConfiguration();

        switch (request.OrderType.ToLower())
        {
            case "market":
                if (request.Side == "buy" && request.QuoteSize.HasValue)
                {
                    config.MarketOrder = new MarketOrderConfig
                    {
                        QuoteSize = request.QuoteSize.Value.ToString()
                    };
                }
                else if (request.BaseSize.HasValue)
                {
                    config.MarketOrder = new MarketOrderConfig
                    {
                        BaseSize = request.BaseSize.Value.ToString()
                    };
                }
                break;

            case "limit":
                config.LimitOrderGTC = new LimitOrderConfig
                {
                    BaseSize = request.BaseSize?.ToString() ?? "0",
                    LimitPrice = request.LimitPrice?.ToString() ?? "0",
                    PostOnly = request.PostOnly
                };
                break;

            case "stop_limit":
                config.StopLimitOrderGTC = new StopLimitOrderConfig
                {
                    BaseSize = request.BaseSize?.ToString() ?? "0",
                    LimitPrice = request.LimitPrice?.ToString() ?? "0",
                    StopPrice = request.StopPrice?.ToString() ?? "0",
                    StopDirection = request.StopDirection ?? "STOP_DIRECTION_STOP_DOWN"
                };
                break;
        }

        return config;
    }

    #endregion
}

#region Request/Response DTOs

public class CreateCoinbaseOrderRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty; // "buy" or "sell"
    public string OrderType { get; set; } = "market"; // "market", "limit", "stop_limit"
    public decimal? BaseSize { get; set; }
    public decimal? QuoteSize { get; set; }
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public string? StopDirection { get; set; }
    public string? TimeInForce { get; set; } = "gtc"; // "gtc", "gtd", "ioc", "fok"
    public bool PostOnly { get; set; } = false;
}

public class CreatePortfolioRequest
{
    public string Name { get; set; } = string.Empty;
}

public class WebSocketSubscriptionRequest
{
    public List<string> Channels { get; set; } = new();
    public List<string> ProductIds { get; set; } = new();
}

public class WebSocketSubscriptionResponse
{
    public string SubscriptionId { get; set; } = string.Empty;
    public List<string> Channels { get; set; } = new();
    public List<string> ProductIds { get; set; } = new();
}

public class ValidationResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion
