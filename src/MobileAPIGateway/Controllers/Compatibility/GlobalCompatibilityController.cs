using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.Compatibility.Global;
using MobileAPIGateway.Services.Compatibility;
using System.Security.Claims;

namespace MobileAPIGateway.Controllers.Compatibility
{
    [ApiController]
    [Route("Global")]
    public class GlobalCompatibilityController : ControllerBase
    {
        private readonly IGlobalCompatibilityService _globalCompatibilityService;
        private readonly ILogger<GlobalCompatibilityController> _logger;

        public GlobalCompatibilityController(
            IGlobalCompatibilityService globalCompatibilityService,
            ILogger<GlobalCompatibilityController> logger)
        {
            _globalCompatibilityService = globalCompatibilityService;
            _logger = logger;
        }

        /// <summary>
        /// Get Countries - Retrieve list of supported countries
        /// </summary>
        [HttpGet("GetCountries")]
        public async Task<ActionResult> GetCountries(
            [FromQuery] string emailAddress,
            [FromQuery] string clientIpAddress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting countries for user: {EmailAddress} from IP: {ClientIpAddress}", 
                    emailAddress, clientIpAddress);

                // Return stub countries data for UAT
                await Task.CompletedTask;
                var result = new
                {
                    status = 200,
                    data = new[]
                    {
                        new { code = "US", name = "United States", enabled = true },
                        new { code = "GB", name = "United Kingdom", enabled = true },
                        new { code = "CA", name = "Canada", enabled = true },
                        new { code = "AU", name = "Australia", enabled = true }
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting countries for user: {EmailAddress}", emailAddress);
                return StatusCode(500, new { status = 500, error = new { message = "Internal server error" } });
            }
        }

        /// <summary>
        /// Get Languages - Retrieve list of supported languages
        /// </summary>
        [HttpGet("GetLanguages")]
        public async Task<ActionResult> GetLanguages(
            [FromQuery] string emailAddress,
            [FromQuery] string clientIpAddress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting languages for user: {EmailAddress} from IP: {ClientIpAddress}", 
                    emailAddress, clientIpAddress);

                // Return stub languages data for UAT
                await Task.CompletedTask;
                var result = new
                {
                    status = 200,
                    data = new[]
                    {
                        new { code = "en", name = "English", enabled = true },
                        new { code = "es", name = "Spanish", enabled = true },
                        new { code = "fr", name = "French", enabled = true },
                        new { code = "de", name = "German", enabled = true }
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting languages for user: {EmailAddress}", emailAddress);
                return StatusCode(500, new { status = 500, error = new { message = "Internal server error" } });
            }
        }

        /// <summary>
        /// Get Cryptocurrencies - Retrieve list of supported cryptocurrencies
        /// </summary>
        [HttpGet("GetCryptoCurrencies")]
        public async Task<ActionResult> GetCryptoCurrencies(
            [FromQuery] string emailAddress,
            [FromQuery] string clientIpAddress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting cryptocurrencies for user: {EmailAddress} from IP: {ClientIpAddress}", 
                    emailAddress, clientIpAddress);

                // Return stub cryptocurrencies data for UAT
                await Task.CompletedTask;
                var result = new
                {
                    status = 200,
                    data = new[]
                    {
                        new { symbol = "BTC", name = "Bitcoin", enabled = true, price = 45000m },
                        new { symbol = "ETH", name = "Ethereum", enabled = true, price = 3000m },
                        new { symbol = "USDT", name = "Tether", enabled = true, price = 1m },
                        new { symbol = "BNB", name = "Binance Coin", enabled = true, price = 400m }
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cryptocurrencies for user: {EmailAddress}", emailAddress);
                return StatusCode(500, new { status = 500, error = new { message = "Internal server error" } });
            }
        }

        /// <summary>
        /// Get User Limits - Retrieve user transaction limits
        /// </summary>
        [HttpGet("GetUserLimits")]
        public async Task<ActionResult> GetUserLimits(
            [FromQuery] string emailAddress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting user limits");

                var userEmail = emailAddress ?? User?.FindFirst(ClaimTypes.Email)?.Value ?? "test@example.com";
                
                // Return stub user limits data for UAT
                await Task.CompletedTask;
                var result = new
                {
                    status = 200,
                    data = new
                    {
                        dailyLimit = 10000m,
                        monthlyLimit = 50000m,
                        yearlyLimit = 200000m,
                        dailyUsed = 1500m,
                        monthlyUsed = 12000m,
                        yearlyUsed = 45000m,
                        currency = "USD"
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user limits");
                return StatusCode(500, new { status = 500, error = new { message = "Internal server error" } });
            }
        }

        /// <summary>
        /// Get Genders - Retrieve list of gender options
        /// </summary>
        [HttpGet("GetGenders")]
        public async Task<ActionResult> GetGenders(
            [FromQuery] string emailAddress,
            [FromQuery] string clientIpAddress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting genders for user: {EmailAddress} from IP: {ClientIpAddress}", 
                    emailAddress, clientIpAddress);

                // Return stub genders data for UAT
                await Task.CompletedTask;
                var result = new
                {
                    status = 200,
                    data = new[]
                    {
                        new { code = "M", name = "Male" },
                        new { code = "F", name = "Female" },
                        new { code = "O", name = "Other" },
                        new { code = "P", name = "Prefer not to say" }
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting genders for user: {EmailAddress}", emailAddress);
                return StatusCode(500, new { status = 500, error = new { message = "Internal server error" } });
            }
        }

        /// <summary>
        /// Get Payment Source Types - Retrieve available payment source types
        /// </summary>
        [HttpGet("GetPaymentSourceTypes")]
        public async Task<ActionResult> GetPaymentSourceTypes(
            [FromQuery] string emailAddress,
            [FromQuery] string clientIpAddress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting payment source types for user: {EmailAddress} from IP: {ClientIpAddress}", 
                    emailAddress, clientIpAddress);

                // Return stub payment source types data for UAT
                await Task.CompletedTask;
                var result = new
                {
                    status = 200,
                    data = new[]
                    {
                        new { type = "CARD", name = "Credit/Debit Card", enabled = true },
                        new { type = "BANK", name = "Bank Transfer", enabled = true },
                        new { type = "CRYPTO", name = "Cryptocurrency", enabled = true },
                        new { type = "PAYPAL", name = "PayPal", enabled = false }
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment source types for user: {EmailAddress}", emailAddress);
                return StatusCode(500, new { status = 500, error = new { message = "Internal server error" } });
            }
        }
    }
}
