using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Services.Compatibility;
using System.Security.Claims;

namespace MobileAPIGateway.Controllers.Compatibility
{
    [ApiController]
    [Route("ShoppingCart")]
    public class ShoppingCartCompatibilityController : ControllerBase
    {
        private readonly IShoppingCartCompatibilityService _shoppingCartCompatibilityService;
        private readonly ILogger<ShoppingCartCompatibilityController> _logger;

        public ShoppingCartCompatibilityController(
            IShoppingCartCompatibilityService shoppingCartCompatibilityService,
            ILogger<ShoppingCartCompatibilityController> logger)
        {
            _shoppingCartCompatibilityService = shoppingCartCompatibilityService;
            _logger = logger;
        }

        [HttpGet("GetCart")]
        public async Task<ActionResult> GetCart([FromQuery] string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting cart for user: {UserId}", userId);
                await Task.CompletedTask;
                
                var result = new
                {
                    status = 200,
                    data = new
                    {
                        cartId = Guid.NewGuid().ToString(),
                        items = new[] { new { id = "1", name = "BTC", quantity = 1 } }
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, new { status = 500, error = new { message = "Internal server error" } });
            }
        }
    }
}
