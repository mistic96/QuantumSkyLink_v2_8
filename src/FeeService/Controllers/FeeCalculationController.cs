using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FeeService.Services.Interfaces;
using FeeService.Models.Requests;
using FeeService.Models.Responses;
using FeeService.Services;

namespace FeeService.Controllers;

/// <summary>
/// Controller for fee calculation and estimation operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class FeeCalculationController : ControllerBase
{
    private readonly IFeeCalculationService _feeCalculationService;
    private readonly ILogger<FeeCalculationController> _logger;

    public FeeCalculationController(
        IFeeCalculationService feeCalculationService,
        ILogger<FeeCalculationController> logger)
    {
        _feeCalculationService = feeCalculationService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate exact fee for a transaction
    /// </summary>
    /// <param name="request">Fee calculation request</param>
    /// <returns>Detailed fee calculation result</returns>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(FeeCalculationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FeeCalculationResponse>> CalculateFee(
        [FromBody] CalculateFeeRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating fee for type {FeeType}, amount {Amount} {Currency}", 
                request.FeeType, request.Amount, request.Currency);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _feeCalculationService.CalculateFeeAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fee calculation request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating fee: {Request}", request);
            return StatusCode(500, "An error occurred while calculating the fee");
        }
    }

    /// <summary>
    /// Estimate fee for a transaction (quick calculation without full processing)
    /// </summary>
    /// <param name="request">Fee estimation request</param>
    /// <returns>Fee estimation result</returns>
    [HttpPost("estimate")]
    [ProducesResponseType(typeof(FeeEstimationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FeeEstimationResponse>> EstimateFee(
        [FromBody] EstimateFeeRequest request)
    {
        try
        {
            _logger.LogInformation("Estimating fee for type {FeeType}, amount {Amount} {Currency}", 
                request.FeeType, request.Amount, request.Currency);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _feeCalculationService.EstimateFeeAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fee estimation request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating fee: {Request}", request);
            return StatusCode(500, "An error occurred while estimating the fee");
        }
    }

    /// <summary>
    /// Get fee configuration for a specific fee type
    /// </summary>
    /// <param name="feeType">Type of fee (e.g., TRANSACTION, WITHDRAWAL, DEPOSIT)</param>
    /// <param name="entityType">Optional entity type for specific configurations</param>
    /// <param name="entityId">Optional entity ID for specific configurations</param>
    /// <returns>Fee configuration details</returns>
    [HttpGet("configurations/{feeType}")]
    [ProducesResponseType(typeof(FeeConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FeeConfigurationResponse>> GetFeeConfiguration(
        [FromRoute] string feeType,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null)
    {
        try
        {
            _logger.LogInformation("Getting fee configuration for type {FeeType}, entity {EntityType}:{EntityId}", 
                feeType, entityType, entityId);

            if (string.IsNullOrWhiteSpace(feeType))
            {
                return BadRequest("Fee type cannot be empty");
            }

            var configuration = await _feeCalculationService.GetFeeConfigurationAsync(feeType, entityType, entityId);
            
            if (configuration == null)
            {
                return NotFound($"Fee configuration not found for type {feeType}");
            }

            return Ok(configuration);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fee configuration request for type {FeeType}", feeType);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fee configuration for type {FeeType}", feeType);
            return StatusCode(500, "An error occurred while retrieving the fee configuration");
        }
    }

    /// <summary>
    /// Create or update fee configuration
    /// </summary>
    /// <param name="request">Fee configuration request</param>
    /// <returns>Created or updated fee configuration</returns>
    [HttpPost("configurations")]
    [ProducesResponseType(typeof(FeeConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FeeConfigurationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FeeConfigurationResponse>> CreateOrUpdateFeeConfiguration(
        [FromBody] CreateFeeConfigurationRequest request)
    {
        try
        {
            _logger.LogInformation("Creating/updating fee configuration for type {FeeType}", request.FeeType);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _feeCalculationService.CreateOrUpdateFeeConfigurationAsync(request);
            
            // Return 201 for new configurations, 200 for updates
            // Note: The service doesn't indicate if it's new or updated, so we'll use 200
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fee configuration request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating fee configuration: {Request}", request);
            return StatusCode(500, "An error occurred while processing the fee configuration");
        }
    }

    /// <summary>
    /// Get all active fee configurations
    /// </summary>
    /// <returns>List of active fee configurations</returns>
    [HttpGet("configurations")]
    [ProducesResponseType(typeof(IEnumerable<FeeConfigurationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FeeConfigurationResponse>>> GetActiveFeeConfigurations()
    {
        try
        {
            _logger.LogInformation("Getting all active fee configurations");

            var configurations = await _feeCalculationService.GetActiveFeeConfigurationsAsync();
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active fee configurations");
            return StatusCode(500, "An error occurred while retrieving fee configurations");
        }
    }

    /// <summary>
    /// Validate fee calculation parameters
    /// </summary>
    /// <param name="feeType">Type of fee to validate</param>
    /// <param name="amount">Amount to validate</param>
    /// <param name="currency">Currency to validate</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> ValidateFeeParameters(
        [FromQuery] string feeType,
        [FromQuery] decimal amount,
        [FromQuery] string currency)
    {
        try
        {
            _logger.LogInformation("Validating fee parameters: type {FeeType}, amount {Amount} {Currency}", 
                feeType, amount, currency);

            if (string.IsNullOrWhiteSpace(feeType))
            {
                return BadRequest("Fee type cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                return BadRequest("Currency cannot be empty");
            }

            if (amount < 0)
            {
                return BadRequest("Amount cannot be negative");
            }

            var isValid = await _feeCalculationService.ValidateFeeParametersAsync(feeType, amount, currency);
            return Ok(isValid);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid validation parameters: {FeeType}, {Amount}, {Currency}", 
                feeType, amount, currency);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating fee parameters: {FeeType}, {Amount}, {Currency}", 
                feeType, amount, currency);
            return StatusCode(500, "An error occurred while validating fee parameters");
        }
    }

    /// <summary>
    /// Get fee calculation history for a user
    /// </summary>
    /// <param name="userId">User ID to get history for</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
    /// <returns>List of fee calculation history</returns>
    [HttpGet("history/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<FeeCalculationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FeeCalculationResponse>>> GetCalculationHistory(
        [FromRoute] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Getting fee calculation history for user {UserId}, page {Page}, size {PageSize}", 
                userId, page, pageSize);

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty");
            }

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var history = await _feeCalculationService.GetCalculationHistoryAsync(userId, page, pageSize);
            return Ok(history);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid calculation history request for user {UserId}", userId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting calculation history for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving calculation history");
        }
    }

    /// <summary>
    /// Apply discounts to a calculated fee (for testing discount logic)
    /// </summary>
    /// <param name="userId">User ID to apply discounts for</param>
    /// <param name="feeType">Type of fee</param>
    /// <param name="calculatedFee">Original calculated fee amount</param>
    /// <returns>Fee amount after applying discounts</returns>
    [HttpPost("apply-discounts")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<decimal>> ApplyDiscounts(
        [FromQuery] Guid userId,
        [FromQuery] string feeType,
        [FromQuery] decimal calculatedFee)
    {
        try
        {
            _logger.LogInformation("Applying discounts for user {UserId}, fee type {FeeType}, amount {Amount}", 
                userId, feeType, calculatedFee);

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(feeType))
            {
                return BadRequest("Fee type cannot be empty");
            }

            if (calculatedFee < 0)
            {
                return BadRequest("Calculated fee cannot be negative");
            }

            var discountedFee = await _feeCalculationService.ApplyDiscountsAsync(userId, feeType, calculatedFee);
            return Ok(discountedFee);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid discount application request for user {UserId}", userId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying discounts for user {UserId}", userId);
            return StatusCode(500, "An error occurred while applying discounts");
        }
    }

    /// <summary>
    /// Calculate fiat rejection fees including wire fees, Square fees, and internal fees
    /// </summary>
    /// <param name="request">Fiat rejection fees calculation request</param>
    /// <returns>Detailed breakdown of fiat rejection fees</returns>
    [HttpPost("calculate-fiat-rejection")]
    [ProducesResponseType(typeof(FiatRejectionFeesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FiatRejectionFeesResponse>> CalculateFiatRejectionFees(
        [FromBody] FiatRejectionFeesRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating fiat rejection fees for amount {Amount} {Currency}, gateway {GatewayType}", 
                request.Amount, request.Currency, request.GatewayType);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            if (string.IsNullOrWhiteSpace(request.Currency))
            {
                return BadRequest("Currency is required");
            }

            var result = await _feeCalculationService.CalculateFiatRejectionFeesAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fiat rejection fees request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating fiat rejection fees: {Request}", request);
            return StatusCode(500, "An error occurred while calculating fiat rejection fees");
        }
    }

    /// <summary>
    /// Calculate crypto rejection fees including network fees and internal fees
    /// </summary>
    /// <param name="request">Crypto rejection fees calculation request</param>
    /// <returns>Detailed breakdown of crypto rejection fees</returns>
    [HttpPost("calculate-crypto-rejection")]
    [ProducesResponseType(typeof(CryptoRejectionFeesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CryptoRejectionFeesResponse>> CalculateCryptoRejectionFees(
        [FromBody] CryptoRejectionFeesRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating crypto rejection fees for amount {Amount} {Cryptocurrency}, network {Network}", 
                request.Amount, request.Cryptocurrency, request.Network);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            if (string.IsNullOrWhiteSpace(request.Cryptocurrency))
            {
                return BadRequest("Cryptocurrency is required");
            }

            var result = await _feeCalculationService.CalculateCryptoRejectionFeesAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid crypto rejection fees request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating crypto rejection fees: {Request}", request);
            return StatusCode(500, "An error occurred while calculating crypto rejection fees");
        }
    }
}
