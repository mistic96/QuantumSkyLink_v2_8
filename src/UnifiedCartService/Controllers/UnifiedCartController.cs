using Microsoft.AspNetCore.Mvc;
using UnifiedCartService.Models.Entities;
using UnifiedCartService.Services;

namespace UnifiedCartService.Controllers;

/// <summary>
/// API controller for unified cart operations
/// Matches the routes expected by ITokenizedCartClient
/// </summary>
[ApiController]
[Route("api")]
public class UnifiedCartController : ControllerBase
{
    private readonly IUnifiedCartService _cartService;
    private readonly ICheckoutOrchestrator _checkoutOrchestrator;
    private readonly ILogger<UnifiedCartController> _logger;

    public UnifiedCartController(
        IUnifiedCartService cartService,
        ICheckoutOrchestrator checkoutOrchestrator,
        ILogger<UnifiedCartController> logger)
    {
        _cartService = cartService;
        _checkoutOrchestrator = checkoutOrchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all carts for a user
    /// </summary>
    [HttpGet("tokenized-carts/user/{userId}")]
    public async Task<IActionResult> GetUserCarts(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var carts = await _cartService.GetUserCartsAsync(userId, cancellationToken);
            return Ok(carts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get carts for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve carts" });
        }
    }

    /// <summary>
    /// Gets a specific cart
    /// </summary>
    [HttpGet("tokenized-carts/user/{userId}/cart/{cartId}")]
    public async Task<IActionResult> GetCartById(string userId, string cartId, CancellationToken cancellationToken)
    {
        try
        {
            var cart = await _cartService.GetCartByIdAsync(userId, cartId, cancellationToken);
            if (cart == null)
                return NotFound(new { error = "Cart not found" });

            return Ok(cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cart {CartId} for user {UserId}", cartId, userId);
            return StatusCode(500, new { error = "Failed to retrieve cart" });
        }
    }

    /// <summary>
    /// Creates a new cart
    /// </summary>
    [HttpPost("tokenized-carts/user/{userId}")]
    public async Task<IActionResult> CreateCart(string userId, [FromBody] CartCreationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var cart = await _cartService.CreateCartAsync(userId, request.Name, request.Currency, cancellationToken);
            
            var response = new CartResponse
            {
                Id = cart.Id,
                Status = "Created",
                Message = "Cart created successfully",
                Timestamp = DateTimeOffset.UtcNow,
                Cart = cart
            };

            return CreatedAtAction(nameof(GetCartById), new { userId, cartId = cart.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create cart for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to create cart" });
        }
    }

    /// <summary>
    /// Updates a cart
    /// </summary>
    [HttpPut("tokenized-carts/user/{userId}/cart/{cartId}")]
    public async Task<IActionResult> UpdateCart(string userId, string cartId, [FromBody] CartUpdateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var cart = await _cartService.UpdateCartAsync(userId, cartId, request.Name, request.Currency, cancellationToken);
            
            var response = new CartResponse
            {
                Id = cart.Id,
                Status = "Updated",
                Message = "Cart updated successfully",
                Timestamp = DateTimeOffset.UtcNow,
                Cart = cart
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update cart {CartId}", cartId);
            return StatusCode(500, new { error = "Failed to update cart" });
        }
    }

    /// <summary>
    /// Adds an item to the cart
    /// </summary>
    [HttpPost("tokenized-carts/user/{userId}/cart/{cartId}/items")]
    public async Task<IActionResult> AddItemToCart(string userId, string cartId, [FromBody] CartItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var item = new UnifiedCartItem
            {
                ListingId = request.ListingId,
                MarketType = request.MarketType,
                TokenId = request.TokenId ?? string.Empty,
                TokenName = request.TokenName ?? string.Empty,
                TokenSymbol = request.TokenSymbol ?? string.Empty,
                AssetType = request.AssetType ?? "Token",
                Quantity = request.Quantity,
                PricePerUnit = request.PricePerUnit,
                Currency = request.Currency,
                SellerId = request.SellerId,
                SellerName = request.SellerName,
                UseEscrow = request.UseEscrow,
                Notes = request.Notes,
                Metadata = request.Metadata
            };

            var cart = await _cartService.AddItemToCartAsync(userId, cartId, item, cancellationToken);
            
            var response = new CartResponse
            {
                Id = cart.Id,
                Status = "ItemAdded",
                Message = "Item added to cart successfully",
                Timestamp = DateTimeOffset.UtcNow,
                Cart = cart
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add item to cart {CartId}", cartId);
            return StatusCode(500, new { error = "Failed to add item to cart" });
        }
    }

    /// <summary>
    /// Removes an item from the cart
    /// </summary>
    [HttpDelete("tokenized-carts/user/{userId}/cart/{cartId}/items/{itemId}")]
    public async Task<IActionResult> RemoveItemFromCart(string userId, string cartId, string itemId, CancellationToken cancellationToken)
    {
        try
        {
            var cart = await _cartService.RemoveItemFromCartAsync(userId, cartId, itemId, cancellationToken);
            
            var response = new CartResponse
            {
                Id = cart.Id,
                Status = "ItemRemoved",
                Message = "Item removed from cart successfully",
                Timestamp = DateTimeOffset.UtcNow,
                Cart = cart
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove item {ItemId} from cart {CartId}", itemId, cartId);
            return StatusCode(500, new { error = "Failed to remove item from cart" });
        }
    }

    /// <summary>
    /// Checks out a cart
    /// </summary>
    [HttpPost("tokenized-carts/user/{userId}/cart/{cartId}/checkout")]
    public async Task<IActionResult> CheckoutCart(string userId, string cartId, [FromBody] CartCheckoutRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var checkoutRequest = new CheckoutRequest
            {
                PaymentMethodId = request.PaymentMethodId,
                WalletCurrency = request.WalletCurrency,
                WalletAddress = request.WalletAddress,
                PaymentCurrency = request.PaymentCurrency,
                ExpressCheckout = request.ExpressCheckout
            };

            var result = await _checkoutOrchestrator.CheckoutCartAsync(userId, cartId, checkoutRequest, cancellationToken);
            
            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            var response = new CartResponse
            {
                Id = cartId,
                Status = "CheckedOut",
                Message = "Cart checked out successfully",
                TransactionId = result.TransactionId,
                Timestamp = result.Timestamp,
                Cart = null // Cart has been deleted from SurrealDB
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to checkout cart {CartId}", cartId);
            return StatusCode(500, new { error = "Failed to checkout cart" });
        }
    }

    /// <summary>
    /// Abandons a cart
    /// </summary>
    [HttpDelete("tokenized-carts/user/{userId}/cart/{cartId}")]
    public async Task<IActionResult> AbandonCart(string userId, string cartId, CancellationToken cancellationToken)
    {
        try
        {
            await _cartService.AbandonCartAsync(userId, cartId, cancellationToken);
            
            var response = new CartResponse
            {
                Id = cartId,
                Status = "Abandoned",
                Message = "Cart abandoned successfully",
                Timestamp = DateTimeOffset.UtcNow,
                Cart = null
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to abandon cart {CartId}", cartId);
            return StatusCode(500, new { error = "Failed to abandon cart" });
        }
    }
}

// Request/Response DTOs to match MobileAPIGateway models
public class CartCreationRequest
{
    public string Name { get; set; } = "Shopping Cart";
    public string Currency { get; set; } = "USD";
}

public class CartUpdateRequest
{
    public string? Name { get; set; }
    public string? Currency { get; set; }
}

public class CartItemRequest
{
    public Guid ListingId { get; set; }
    public MarketType MarketType { get; set; }
    public string? TokenId { get; set; }
    public string? TokenName { get; set; }
    public string? TokenSymbol { get; set; }
    public string? AssetType { get; set; }
    public decimal Quantity { get; set; }
    public decimal PricePerUnit { get; set; }
    public string Currency { get; set; } = "USD";
    public Guid? SellerId { get; set; }
    public string? SellerName { get; set; }
    public bool UseEscrow { get; set; }
    public string? Notes { get; set; }
    public string? Metadata { get; set; }
}

public class CartCheckoutRequest
{
    public string PaymentMethodId { get; set; }
    public string? WalletCurrency { get; set; }
    public string WalletAddress { get; set; }
    public string PaymentCurrency { get; set; } = "USD";
    public bool ExpressCheckout { get; set; }
}

public class CartResponse
{
    public string Id { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public string? TransactionId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public UnifiedCart? Cart { get; set; }
}