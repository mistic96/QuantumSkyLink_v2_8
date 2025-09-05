using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Services.Compatibility;
using System.Security.Claims;

namespace MobileAPIGateway.Controllers.Compatibility
{
    [ApiController]
    [Route("Search")]
    public class SearchCompatibilityController : ControllerBase
    {
        private readonly ISearchCompatibilityService _searchCompatibilityService;
        private readonly ILogger<SearchCompatibilityController> _logger;

        public SearchCompatibilityController(
            ISearchCompatibilityService searchCompatibilityService,
            ILogger<SearchCompatibilityController> logger)
        {
            _searchCompatibilityService = searchCompatibilityService;
            _logger = logger;
        }

        [HttpGet("Search")]
        public async Task<ActionResult> Search([FromQuery] string query, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Searching for: {Query}", query);
                await Task.CompletedTask;
                
                var result = new
                {
                    status = 200,
                    data = new[]
                    {
                        new { type = "crypto", name = "Bitcoin", symbol = "BTC" },
                        new { type = "crypto", name = "Ethereum", symbol = "ETH" }
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching");
                return StatusCode(500, new { status = 500, error = new { message = "Internal server error" } });
            }
        }
    }
}
