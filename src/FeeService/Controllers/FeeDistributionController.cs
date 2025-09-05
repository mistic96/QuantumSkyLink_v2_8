using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FeeService.Services.Interfaces;
using FeeService.Models.Requests;
using FeeService.Models.Responses;

namespace FeeService.Controllers;

/// <summary>
/// Controller for fee distribution and settlement operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class FeeDistributionController : ControllerBase
{
    private readonly IFeeDistributionService _feeDistributionService;
    private readonly ILogger<FeeDistributionController> _logger;

    public FeeDistributionController(
        IFeeDistributionService feeDistributionService,
        ILogger<FeeDistributionController> logger)
    {
        _feeDistributionService = feeDistributionService;
        _logger = logger;
    }

    /// <summary>
    /// Distribute fees according to distribution rules
    /// </summary>
    /// <param name="request">Fee distribution request</param>
    /// <returns>List of fee distributions created</returns>
    [HttpPost("distribute")]
    [ProducesResponseType(typeof(IEnumerable<FeeDistributionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FeeDistributionResponse>>> DistributeFees(
        [FromBody] DistributeFeesRequest request)
    {
        try
        {
            _logger.LogInformation("Distributing fees for transaction {TransactionId}", 
                request.TransactionId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var distributions = await _feeDistributionService.DistributeFeesAsync(request);
            return Ok(distributions);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fee distribution request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error distributing fees: {Request}", request);
            return StatusCode(500, "An error occurred while distributing fees");
        }
    }

    /// <summary>
    /// Create or update distribution rule
    /// </summary>
    /// <param name="request">Distribution rule request</param>
    /// <returns>Created or updated distribution rule</returns>
    [HttpPost("rules")]
    [ProducesResponseType(typeof(DistributionRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DistributionRuleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DistributionRuleResponse>> CreateOrUpdateDistributionRule(
        [FromBody] CreateDistributionRuleRequest request)
    {
        try
        {
            _logger.LogInformation("Creating/updating distribution rule for fee type {FeeType}", 
                request.FeeType);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rule = await _feeDistributionService.CreateOrUpdateDistributionRuleAsync(request);
            
            // Return 200 for updates, could be enhanced to detect new vs update
            return Ok(rule);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid distribution rule request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating distribution rule: {Request}", request);
            return StatusCode(500, "An error occurred while processing the distribution rule");
        }
    }

    /// <summary>
    /// Get distribution rules for a fee type
    /// </summary>
    /// <param name="feeType">Fee type to get rules for</param>
    /// <param name="activeOnly">Whether to return only active rules (default: true)</param>
    /// <returns>List of distribution rules</returns>
    [HttpGet("rules/{feeType}")]
    [ProducesResponseType(typeof(IEnumerable<DistributionRuleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DistributionRuleResponse>>> GetDistributionRules(
        [FromRoute] string feeType,
        [FromQuery] bool activeOnly = true)
    {
        try
        {
            _logger.LogInformation("Getting distribution rules for fee type {FeeType}, active only: {ActiveOnly}", 
                feeType, activeOnly);

            if (string.IsNullOrWhiteSpace(feeType))
            {
                return BadRequest("Fee type cannot be empty");
            }

            var rules = await _feeDistributionService.GetDistributionRulesAsync(feeType, activeOnly);
            return Ok(rules);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid distribution rules request for fee type {FeeType}", feeType);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distribution rules for fee type {FeeType}", feeType);
            return StatusCode(500, "An error occurred while retrieving distribution rules");
        }
    }

    /// <summary>
    /// Get distribution history for a transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID to get distribution history for</param>
    /// <returns>List of distributions for the transaction</returns>
    [HttpGet("history/{transactionId}")]
    [ProducesResponseType(typeof(IEnumerable<FeeDistributionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FeeDistributionResponse>>> GetDistributionHistory(
        [FromRoute] Guid transactionId)
    {
        try
        {
            _logger.LogInformation("Getting distribution history for transaction {TransactionId}", 
                transactionId);

            if (transactionId == Guid.Empty)
            {
                return BadRequest("Transaction ID cannot be empty");
            }

            var history = await _feeDistributionService.GetDistributionHistoryAsync(transactionId);
            return Ok(history);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid distribution history request for transaction {TransactionId}", 
                transactionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distribution history for transaction {TransactionId}", 
                transactionId);
            return StatusCode(500, "An error occurred while retrieving distribution history");
        }
    }

    /// <summary>
    /// Process settlement for distributions
    /// </summary>
    /// <param name="request">Settlement processing request</param>
    /// <returns>Settlement result</returns>
    [HttpPost("settlement")]
    [ProducesResponseType(typeof(SettlementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SettlementResponse>> ProcessSettlement(
        [FromBody] ProcessSettlementRequest request)
    {
        try
        {
            _logger.LogInformation("Processing settlement for {DistributionCount} distributions", 
                request.DistributionIds?.Count() ?? 0);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var settlement = await _feeDistributionService.ProcessSettlementAsync(request);
            return Ok(settlement);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid settlement request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing settlement: {Request}", request);
            return StatusCode(500, "An error occurred while processing the settlement");
        }
    }

    /// <summary>
    /// Get pending distributions
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
    /// <returns>List of pending distributions</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<FeeDistributionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FeeDistributionResponse>>> GetPendingDistributions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Getting pending distributions, page {Page}, size {PageSize}", 
                page, pageSize);

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var pendingDistributions = await _feeDistributionService.GetPendingDistributionsAsync(page, pageSize);
            return Ok(pendingDistributions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending distributions");
            return StatusCode(500, "An error occurred while retrieving pending distributions");
        }
    }

    /// <summary>
    /// Update distribution status
    /// </summary>
    /// <param name="distributionId">Distribution ID to update</param>
    /// <param name="status">New status for the distribution</param>
    /// <param name="reason">Optional reason for the status change</param>
    /// <returns>Updated distribution details</returns>
    [HttpPut("{distributionId}/status")]
    [ProducesResponseType(typeof(FeeDistributionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FeeDistributionResponse>> UpdateDistributionStatus(
        [FromRoute] Guid distributionId,
        [FromQuery] string status,
        [FromQuery] string? reason = null)
    {
        try
        {
            _logger.LogInformation("Updating distribution {DistributionId} status to {Status}", 
                distributionId, status);

            if (distributionId == Guid.Empty)
            {
                return BadRequest("Distribution ID cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest("Status cannot be empty");
            }

            var result = await _feeDistributionService.UpdateDistributionStatusAsync(distributionId, status, reason);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid status update request for distribution {DistributionId}", 
                distributionId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot update status for distribution {DistributionId}", distributionId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating distribution status for {DistributionId}", distributionId);
            return StatusCode(500, "An error occurred while updating the distribution status");
        }
    }

    /// <summary>
    /// Validate distribution rules for a fee type
    /// </summary>
    /// <param name="feeType">Fee type to validate rules for</param>
    /// <returns>Validation result</returns>
    [HttpGet("rules/{feeType}/validate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> ValidateDistributionRules(
        [FromRoute] string feeType)
    {
        try
        {
            _logger.LogInformation("Validating distribution rules for fee type {FeeType}", feeType);

            if (string.IsNullOrWhiteSpace(feeType))
            {
                return BadRequest("Fee type cannot be empty");
            }

            var isValid = await _feeDistributionService.ValidateDistributionRulesAsync(feeType);
            return Ok(isValid);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid validation request for fee type {FeeType}", feeType);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating distribution rules for fee type {FeeType}", feeType);
            return StatusCode(500, "An error occurred while validating distribution rules");
        }
    }

    /// <summary>
    /// Get distribution statistics for a date range
    /// </summary>
    /// <param name="fromDate">Start date for statistics</param>
    /// <param name="toDate">End date for statistics</param>
    /// <param name="feeType">Optional fee type filter</param>
    /// <returns>Distribution statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(DistributionStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DistributionStatisticsResponse>> GetDistributionStatistics(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? feeType = null)
    {
        try
        {
            _logger.LogInformation("Getting distribution statistics from {FromDate} to {ToDate}, fee type: {FeeType}", 
                fromDate, toDate, feeType);

            if (fromDate == default)
            {
                return BadRequest("From date is required");
            }

            if (toDate == default)
            {
                return BadRequest("To date is required");
            }

            if (fromDate > toDate)
            {
                return BadRequest("From date cannot be after to date");
            }

            if (toDate > DateTime.UtcNow)
            {
                return BadRequest("To date cannot be in the future");
            }

            var statistics = await _feeDistributionService.GetDistributionStatisticsAsync(fromDate, toDate, feeType);
            return Ok(statistics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid statistics request: {FromDate} to {ToDate}, fee type: {FeeType}", 
                fromDate, toDate, feeType);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distribution statistics: {FromDate} to {ToDate}, fee type: {FeeType}", 
                fromDate, toDate, feeType);
            return StatusCode(500, "An error occurred while retrieving distribution statistics");
        }
    }
}
