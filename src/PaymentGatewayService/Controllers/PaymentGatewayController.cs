using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Services.Interfaces;

namespace PaymentGatewayService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentGatewayController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly ILogger<PaymentGatewayController> _logger;

    public PaymentGatewayController(
        IPaymentGatewayService paymentGatewayService,
        ILogger<PaymentGatewayController> logger)
    {
        _paymentGatewayService = paymentGatewayService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active payment gateways
    /// </summary>
    /// <returns>List of active payment gateways</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentGatewayResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PaymentGatewayResponse>>> GetGateways()
    {
        try
        {
            _logger.LogInformation("Retrieving all active payment gateways");

            var gateways = await _paymentGatewayService.GetActiveGatewaysAsync();
            
            _logger.LogInformation("Retrieved {Count} active payment gateways", gateways.Count());
            return Ok(gateways);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment gateways");
            return StatusCode(500, "An error occurred while retrieving payment gateways");
        }
    }

    /// <summary>
    /// Get a specific payment gateway by ID
    /// </summary>
    /// <param name="id">Gateway ID</param>
    /// <returns>Payment gateway details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentGatewayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentGatewayResponse>> GetGateway(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving payment gateway {GatewayId}", id);

            var gateway = await _paymentGatewayService.GetGatewayAsync(id);
            if (gateway == null)
            {
                _logger.LogWarning("Payment gateway {GatewayId} not found", id);
                return NotFound($"Payment gateway with ID {id} not found");
            }

            return Ok(gateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment gateway {GatewayId}", id);
            return StatusCode(500, "An error occurred while retrieving the payment gateway");
        }
    }

    /// <summary>
    /// Get the best gateway for a payment
    /// </summary>
    /// <param name="amount">Payment amount</param>
    /// <param name="currency">Payment currency</param>
    /// <param name="paymentType">Payment type</param>
    /// <param name="country">User's country (optional)</param>
    /// <returns>Recommended payment gateway</returns>
    [HttpGet("best")]
    [ProducesResponseType(typeof(PaymentGatewayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentGatewayResponse>> GetBestGateway(
        [FromQuery] decimal amount,
        [FromQuery] string currency,
        [FromQuery] int paymentType,
        [FromQuery] string? country = null)
    {
        try
        {
            _logger.LogInformation("Finding best gateway for amount {Amount} {Currency}, type {PaymentType}", 
                amount, currency, paymentType);

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than 0");
            }

            if (string.IsNullOrEmpty(currency) || currency.Length != 3)
            {
                return BadRequest("Currency must be a valid 3-letter ISO code");
            }

            var gateway = await _paymentGatewayService.GetBestGatewayAsync(
                amount, currency, (PaymentGatewayService.Models.PaymentType)paymentType, country);
            
            if (gateway == null)
            {
                _logger.LogWarning("No suitable gateway found for amount {Amount} {Currency}", amount, currency);
                return NotFound("No suitable payment gateway found for the specified criteria");
            }

            return Ok(gateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding best gateway for amount {Amount} {Currency}", amount, currency);
            return StatusCode(500, "An error occurred while finding the best payment gateway");
        }
    }

    /// <summary>
    /// Create a new payment gateway
    /// </summary>
    /// <param name="request">Gateway creation details</param>
    /// <returns>Created gateway</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaymentGatewayResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentGatewayResponse>> CreateGateway([FromBody] CreatePaymentGatewayRequest request)
    {
        try
        {
            _logger.LogInformation("Creating payment gateway {GatewayName}", request.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var gateway = await _paymentGatewayService.CreateGatewayAsync(request);
            
            _logger.LogInformation("Payment gateway {GatewayId} created successfully", gateway.Id);
            return CreatedAtAction(nameof(GetGateway), new { id = gateway.Id }, gateway);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating payment gateway");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment gateway {GatewayName}", request.Name);
            return StatusCode(500, "An error occurred while creating the payment gateway");
        }
    }

    /// <summary>
    /// Update an existing payment gateway
    /// </summary>
    /// <param name="id">Gateway ID</param>
    /// <param name="request">Gateway update details</param>
    /// <returns>Updated gateway</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaymentGatewayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentGatewayResponse>> UpdateGateway(Guid id, [FromBody] UpdatePaymentGatewayRequest request)
    {
        try
        {
            _logger.LogInformation("Updating payment gateway {GatewayId}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Set the gateway ID from the route
            request.GatewayId = id;

            var gateway = await _paymentGatewayService.UpdateGatewayAsync(request);
            
            _logger.LogInformation("Payment gateway {GatewayId} updated successfully", id);
            return Ok(gateway);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating payment gateway");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment gateway {GatewayId}", id);
            return StatusCode(500, "An error occurred while updating the payment gateway");
        }
    }

    /// <summary>
    /// Enable or disable a payment gateway
    /// </summary>
    /// <param name="id">Gateway ID</param>
    /// <param name="enabled">Whether to enable or disable the gateway</param>
    /// <returns>Updated gateway</returns>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaymentGatewayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentGatewayResponse>> UpdateGatewayStatus(Guid id, [FromQuery] bool enabled)
    {
        try
        {
            _logger.LogInformation("Updating payment gateway {GatewayId} status to {Status}", 
                id, enabled ? "enabled" : "disabled");

            var gateway = await _paymentGatewayService.SetGatewayStatusAsync(id, enabled);
            
            _logger.LogInformation("Payment gateway {GatewayId} status updated successfully", id);
            return Ok(gateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for payment gateway {GatewayId}", id);
            return StatusCode(500, "An error occurred while updating gateway status");
        }
    }

    /// <summary>
    /// Test a payment gateway connection
    /// </summary>
    /// <param name="id">Gateway ID</param>
    /// <returns>Test result</returns>
    [HttpPost("{id:guid}/test")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GatewayTestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GatewayTestResponse>> TestGateway(Guid id)
    {
        try
        {
            _logger.LogInformation("Testing payment gateway {GatewayId}", id);

            var testResult = await _paymentGatewayService.TestGatewayAsync(id);
            
            _logger.LogInformation("Payment gateway {GatewayId} test completed: {IsSuccessful}", 
                id, testResult.IsSuccessful);
            return Ok(testResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing payment gateway {GatewayId}", id);
            return StatusCode(500, "An error occurred while testing the payment gateway");
        }
    }

    /// <summary>
    /// Get gateway statistics
    /// </summary>
    /// <param name="id">Gateway ID</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <returns>Gateway statistics</returns>
    [HttpGet("{id:guid}/statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GatewayStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GatewayStatisticsResponse>> GetGatewayStatistics(
        Guid id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation("Retrieving statistics for payment gateway {GatewayId}", id);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var statistics = await _paymentGatewayService.GetGatewayStatisticsAsync(id, fromDate, toDate);
            
            _logger.LogInformation("Retrieved statistics for payment gateway {GatewayId}", id);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for payment gateway {GatewayId}", id);
            return StatusCode(500, "An error occurred while retrieving gateway statistics");
        }
    }

    /// <summary>
    /// Get supported currencies for a gateway
    /// </summary>
    /// <param name="id">Gateway ID</param>
    /// <returns>List of supported currencies</returns>
    [HttpGet("{id:guid}/currencies")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetSupportedCurrencies(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving supported currencies for payment gateway {GatewayId}", id);

            var currencies = await _paymentGatewayService.GetSupportedCurrenciesAsync(id);
            
            _logger.LogInformation("Retrieved {Count} supported currencies for gateway {GatewayId}", 
                currencies.Count(), id);
            return Ok(currencies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported currencies for gateway {GatewayId}", id);
            return StatusCode(500, "An error occurred while retrieving supported currencies");
        }
    }

    /// <summary>
    /// Get supported countries for a gateway
    /// </summary>
    /// <param name="id">Gateway ID</param>
    /// <returns>List of supported countries</returns>
    [HttpGet("{id:guid}/countries")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetSupportedCountries(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving supported countries for payment gateway {GatewayId}", id);

            var countries = await _paymentGatewayService.GetSupportedCountriesAsync(id);
            
            _logger.LogInformation("Retrieved {Count} supported countries for gateway {GatewayId}", 
                countries.Count(), id);
            return Ok(countries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported countries for gateway {GatewayId}", id);
            return StatusCode(500, "An error occurred while retrieving supported countries");
        }
    }
}
