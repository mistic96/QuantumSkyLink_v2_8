using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.Compatibility.Markets;
using MobileAPIGateway.Services.Compatibility;
using System.Security.Claims;

namespace MobileAPIGateway.Controllers.Compatibility
{
    [ApiController]
    [Route("Markets")]
    public class MarketsCompatibilityController : ControllerBase
    {
        private readonly IMarketsCompatibilityService _marketsCompatibilityService;
        private readonly ILogger<MarketsCompatibilityController> _logger;

        public MarketsCompatibilityController(
            IMarketsCompatibilityService marketsCompatibilityService,
            ILogger<MarketsCompatibilityController> logger)
        {
            _marketsCompatibilityService = marketsCompatibilityService;
            _logger = logger;
        }

        [HttpGet("GetMarkets")]
        public async Task<ActionResult> GetMarkets(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting markets");
                await Task.CompletedTask;
                
                var result = new
                {
                    status = 200,
                    data = new[]
                    {
                        new { symbol = "BTC/USD", price = 45000m, change = 2.5m },
                        new { symbol = "ETH/USD", price = 3000m, change = 1.8m }
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting markets");
                return StatusCode(500, new { status = 500, error = new { message = "Internal server error" } });
            }
        }
    }
}
