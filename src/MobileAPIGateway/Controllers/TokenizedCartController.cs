using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MobileAPIGateway.Models.TokenizedCart;
using MobileAPIGateway.Services;

namespace MobileAPIGateway.Controllers;

/// <summary>
/// Controller for tokenized cart operations
/// </summary>
[ApiController]
[Route("api/tokenized-carts")]
[Authorize]
public class TokenizedCartController : BaseController
{
    private readonly ITokenizedCartService _tokenizedCartService;
    private readonly ILogger<TokenizedCartController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenizedCartController"/> class
    /// </summary>
    /// <param name="tokenizedCartService">Tokenized cart service</param>
    /// <param name="logger">Logger</param>
    public TokenizedCartController(ITokenizedCartService tokenizedCartService, ILogger<TokenizedCartController> logger)
    {
        _tokenizedCartService = tokenizedCartService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all tokenized carts for the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tokenized carts</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TokenizedCart>), 200)]
    public async Task<IActionResult> GetUserCarts(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var carts = await _tokenizedCartService.GetUserCartsAsync(userId, cancellationToken);
        return Ok(carts);
    }

    /// <summary>
    /// Gets a tokenized cart by ID
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tokenized cart</returns>
    [HttpGet("{cartId}")]
    [ProducesResponseType(typeof(TokenizedCart), 200)]
    public async Task<IActionResult> GetCartById(string cartId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var cart = await _tokenizedCartService.GetCartByIdAsync(userId, cartId, cancellationToken);
        return Ok(cart);
    }

    /// <summary>
    /// Creates a new tokenized cart
    /// </summary>
    /// <param name="request">Cart creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CartResponse), 201)]
    public async Task<IActionResult> CreateCart([FromBody] CartCreationRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var response = await _tokenizedCartService.CreateCartAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetCartById), new { cartId = response.Id }, response);
    }

    /// <summary>
    /// Updates a tokenized cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Cart update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [HttpPut("{cartId}")]
    [ProducesResponseType(typeof(CartResponse), 200)]
    public async Task<IActionResult> UpdateCart(string cartId, [FromBody] CartUpdateRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var response = await _tokenizedCartService.UpdateCartAsync(userId, cartId, request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Adds an item to a tokenized cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Cart item request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [HttpPost("{cartId}/items")]
    [ProducesResponseType(typeof(CartResponse), 200)]
    public async Task<IActionResult> AddItemToCart(string cartId, [FromBody] CartItemRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var response = await _tokenizedCartService.AddItemToCartAsync(userId, cartId, request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Removes an item from a tokenized cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="itemId">Item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [HttpDelete("{cartId}/items/{itemId}")]
    [ProducesResponseType(typeof(CartResponse), 200)]
    public async Task<IActionResult> RemoveItemFromCart(string cartId, string itemId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var response = await _tokenizedCartService.RemoveItemFromCartAsync(userId, cartId, itemId, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Checks out a tokenized cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Cart checkout request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [HttpPost("{cartId}/checkout")]
    [ProducesResponseType(typeof(CartResponse), 200)]
    public async Task<IActionResult> CheckoutCart(string cartId, [FromBody] CartCheckoutRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var response = await _tokenizedCartService.CheckoutCartAsync(userId, cartId, request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Abandons a tokenized cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [HttpDelete("{cartId}")]
    [ProducesResponseType(typeof(CartResponse), 200)]
    public async Task<IActionResult> AbandonCart(string cartId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var response = await _tokenizedCartService.AbandonCartAsync(userId, cartId, cancellationToken);
        return Ok(response);
    }
   

   // Get User Id from the current context
    private string GetCurrentUserId()
    {

    if (User == null || !User.Identity.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
    // Assuming the user ID is stored in a claim named "sub"

        return User?.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }
}
