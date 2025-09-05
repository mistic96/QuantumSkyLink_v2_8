//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using MobileAPIGateway.Controllers;
//using MobileAPIGateway.Models.Compatibility.Carts;
//using MobileAPIGateway.Services.Compatibility;

//namespace MobileAPIGateway.Controllers.Compatibility;

///// <summary>
///// Controller for carts compatibility operations
///// </summary>
//[ApiController]
//[Route("api/v1/compatibility/carts")]
//[Authorize]
//public class CartsCompatibilityController : BaseController
//{
//    private readonly ICartsCompatibilityService _cartsCompatibilityService;
//    private readonly ILogger<CartsCompatibilityController> _logger;
    
//    /// <summary>
//    /// Initializes a new instance of the <see cref="CartsCompatibilityController"/> class
//    /// </summary>
//    /// <param name="cartsCompatibilityService">The carts compatibility service</param>
//    /// <param name="logger">The logger</param>
//    public CartsCompatibilityController(
//        ICartsCompatibilityService cartsCompatibilityService,
//        ILogger<CartsCompatibilityController> logger)
//    {
//        _cartsCompatibilityService = cartsCompatibilityService ?? throw new ArgumentNullException(nameof(cartsCompatibilityService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }
    
//    /// <summary>
//    /// Creates a cart
//    /// </summary>
//    /// <param name="request">The create cart request</param>
//    /// <returns>The create cart response</returns>
//    [HttpPost("create")]
//    public async Task<ActionResult<CreateCartCompatibilityResponse>> CreateCartAsync([FromBody] CreateCartCompatibilityRequest request)
//    {
//        try
//        {
//            _logger.LogInformation("Creating cart for customer {CustomerId}", request.CustomerId);
            
//            // Set the customer ID from the authenticated user if not provided
//            if (string.IsNullOrEmpty(request.CustomerId))
//            {
//                request.CustomerId = UserEmail;
//            }
            
//            var response = await _cartsCompatibilityService.CreateCartAsync(request);
            
//            if (!response.IsSuccessful)
//            {
//                return BadRequest(response);
//            }
            
//            return Ok(response);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error creating cart for customer {CustomerId}", request.CustomerId);
//            return StatusCode(500, new CreateCartCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = $"Error creating cart: {ex.Message}"
//            });
//        }
//    }
    
//    /// <summary>
//    /// Updates a cart
//    /// </summary>
//    /// <param name="request">The update cart request</param>
//    /// <returns>The update cart response</returns>
//    [HttpPost("update")]
//    public async Task<ActionResult<UpdateCartCompatibilityResponse>> UpdateCartAsync([FromBody] UpdateCartCompatibilityRequest request)
//    {
//        try
//        {
//            _logger.LogInformation("Updating cart {CartId} for customer {CustomerId}", request.CartId, request.CustomerId);
            
//            // Set the customer ID from the authenticated user if not provided
//            if (string.IsNullOrEmpty(request.CustomerId))
//            {
//                request.CustomerId = UserEmail;
//            }
            
//            var response = await _cartsCompatibilityService.UpdateCartAsync(request);
            
//            if (!response.IsSuccessful)
//            {
//                return BadRequest(response);
//            }
            
//            return Ok(response);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error updating cart {CartId} for customer {CustomerId}", request.CartId, request.CustomerId);
//            return StatusCode(500, new UpdateCartCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = $"Error updating cart: {ex.Message}"
//            });
//        }
//    }
    
//    /// <summary>
//    /// Checks out a cart
//    /// </summary>
//    /// <param name="request">The checkout cart request</param>
//    /// <returns>The checkout cart response</returns>
//    [HttpPost("checkout")]
//    public async Task<ActionResult<CheckoutCartCompatibilityResponse>> CheckoutCartAsync([FromBody] CheckoutCartCompatibilityRequest request)
//    {
//        try
//        {
//            _logger.LogInformation("Checking out cart {CartId} for customer {CustomerId}", request.CartId, request.CustomerId);
            
//            // Set the customer ID from the authenticated user if not provided
//            if (string.IsNullOrEmpty(request.CustomerId))
//            {
//                request.CustomerId = UserEmail;
//            }
            
//            var response = await _cartsCompatibilityService.CheckoutCartAsync(request);
            
//            if (!response.IsSuccessful)
//            {
//                return BadRequest(response);
//            }
            
//            return Ok(response);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error checking out cart {CartId} for customer {CustomerId}", request.CartId, request.CustomerId);
//            return StatusCode(500, new CheckoutCartCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = $"Error checking out cart: {ex.Message}"
//            });
//        }
//    }
    
//    /// <summary>
//    /// Gets a cart
//    /// </summary>
//    /// <param name="cartId">The cart ID</param>
//    /// <param name="customerId">The customer ID (optional, will use authenticated user if not provided)</param>
//    /// <returns>The cart response</returns>
//    [HttpGet("{cartId}")]
//    public async Task<ActionResult<CreateCartCompatibilityResponse>> GetCartAsync(string cartId, [FromQuery] string? customerId = null)
//    {
//        try
//        {
//            // Set the customer ID from the authenticated user if not provided
//            if (string.IsNullOrEmpty(customerId))
//            {
//                customerId = UserEmail;
//            }
            
//            _logger.LogInformation("Getting cart {CartId} for customer {CustomerId}", cartId, customerId);
            
//            var response = await _cartsCompatibilityService.GetCartAsync(cartId, customerId);
            
//            if (!response.IsSuccessful)
//            {
//                return BadRequest(response);
//            }
            
//            return Ok(response);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting cart {CartId} for customer {CustomerId}", cartId, customerId);
//            return StatusCode(500, new CreateCartCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = $"Error getting cart: {ex.Message}"
//            });
//        }
//    }
    
//    /// <summary>
//    /// Deletes a cart
//    /// </summary>
//    /// <param name="cartId">The cart ID</param>
//    /// <param name="customerId">The customer ID (optional, will use authenticated user if not provided)</param>
//    /// <returns>True if the cart was deleted successfully, otherwise false</returns>
//    [HttpDelete("{cartId}")]
//    public async Task<ActionResult<bool>> DeleteCartAsync(string cartId, [FromQuery] string? customerId = null)
//    {
//        try
//        {
//            // Set the customer ID from the authenticated user if not provided
//            if (string.IsNullOrEmpty(customerId))
//            {
//                customerId = UserEmail;
//            }
            
//            _logger.LogInformation("Deleting cart {CartId} for customer {CustomerId}", cartId, customerId);
            
//            var result = await _cartsCompatibilityService.DeleteCartAsync(cartId, customerId);
            
//            if (!result)
//            {
//                return BadRequest(false);
//            }
            
//            return Ok(true);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error deleting cart {CartId} for customer {CustomerId}", cartId, customerId);
//            return StatusCode(500, false);
//        }
//    }
//}
