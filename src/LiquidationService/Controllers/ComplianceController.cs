using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LiquidationService.Models.Responses;
using LiquidationService.Services.Interfaces;
using System.Security.Claims;

namespace LiquidationService.Controllers;

/// <summary>
/// Controller for compliance checks and risk management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplianceController : ControllerBase
{
    private readonly IComplianceService _complianceService;
    private readonly ILogger<ComplianceController> _logger;

    public ComplianceController(
        IComplianceService complianceService,
        ILogger<ComplianceController> logger)
    {
        _complianceService = complianceService;
        _logger = logger;
    }

    /// <summary>
    /// Perform KYC verification for a liquidation request
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>KYC compliance check result</returns>
    [HttpPost("kyc/{liquidationRequestId:guid}")]
    [ProducesResponseType(typeof(ComplianceCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ComplianceCheckResponse>> PerformKycCheck(
        Guid liquidationRequestId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Performing KYC check for liquidation request {RequestId} by user {UserId}", 
                liquidationRequestId, currentUserId);

            var result = await _complianceService.PerformKycCheckAsync(liquidationRequestId, currentUserId, cancellationToken);
            
            _logger.LogInformation("KYC check completed for liquidation request {RequestId} with result {Result}", 
                liquidationRequestId, result.Result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for KYC check");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing KYC check for liquidation request {RequestId}", liquidationRequestId);
            return StatusCode(500, "An error occurred while performing the KYC check");
        }
    }

    /// <summary>
    /// Perform AML (Anti-Money Laundering) check
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="amount">Transaction amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AML compliance check result</returns>
    [HttpPost("aml/{liquidationRequestId:guid}")]
    [ProducesResponseType(typeof(ComplianceCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ComplianceCheckResponse>> PerformAmlCheck(
        Guid liquidationRequestId,
        [FromQuery] decimal amount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Performing AML check for liquidation request {RequestId} amount {Amount} by user {UserId}", 
                liquidationRequestId, amount, currentUserId);

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            var result = await _complianceService.PerformAmlCheckAsync(liquidationRequestId, currentUserId, amount, cancellationToken);
            
            _logger.LogInformation("AML check completed for liquidation request {RequestId} with result {Result}", 
                liquidationRequestId, result.Result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for AML check");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing AML check for liquidation request {RequestId}", liquidationRequestId);
            return StatusCode(500, "An error occurred while performing the AML check");
        }
    }

    /// <summary>
    /// Perform sanctions screening
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sanctions screening result</returns>
    [HttpPost("sanctions/{liquidationRequestId:guid}")]
    [ProducesResponseType(typeof(ComplianceCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ComplianceCheckResponse>> PerformSanctionsScreening(
        Guid liquidationRequestId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Performing sanctions screening for liquidation request {RequestId} by user {UserId}", 
                liquidationRequestId, currentUserId);

            var result = await _complianceService.PerformSanctionsScreeningAsync(liquidationRequestId, currentUserId, cancellationToken);
            
            _logger.LogInformation("Sanctions screening completed for liquidation request {RequestId} with result {Result}", 
                liquidationRequestId, result.Result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for sanctions screening");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing sanctions screening for liquidation request {RequestId}", liquidationRequestId);
            return StatusCode(500, "An error occurred while performing sanctions screening");
        }
    }

    /// <summary>
    /// Perform risk assessment for a liquidation request
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="assetSymbol">Asset being liquidated</param>
    /// <param name="amount">Amount being liquidated</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Risk assessment result</returns>
    [HttpPost("risk-assessment/{liquidationRequestId:guid}")]
    [ProducesResponseType(typeof(ComplianceCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ComplianceCheckResponse>> PerformRiskAssessment(
        Guid liquidationRequestId,
        [FromQuery] string assetSymbol,
        [FromQuery] decimal amount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Performing risk assessment for liquidation request {RequestId} - Asset: {AssetSymbol}, Amount: {Amount}", 
                liquidationRequestId, assetSymbol, amount);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            var result = await _complianceService.PerformRiskAssessmentAsync(
                liquidationRequestId, currentUserId, assetSymbol, amount, cancellationToken);
            
            _logger.LogInformation("Risk assessment completed for liquidation request {RequestId} with result {Result}", 
                liquidationRequestId, result.Result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for risk assessment");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing risk assessment for liquidation request {RequestId}", liquidationRequestId);
            return StatusCode(500, "An error occurred while performing risk assessment");
        }
    }

    /// <summary>
    /// Get compliance check by ID
    /// </summary>
    /// <param name="id">Compliance check ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance check details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ComplianceCheckResponse>> GetComplianceCheck(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving compliance check {CheckId}", id);

            var result = await _complianceService.GetComplianceCheckAsync(id, cancellationToken);
            if (result == null)
            {
                _logger.LogWarning("Compliance check {CheckId} not found", id);
                return NotFound($"Compliance check with ID {id} not found");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance check {CheckId}", id);
            return StatusCode(500, "An error occurred while retrieving the compliance check");
        }
    }

    /// <summary>
    /// Get all compliance checks for a liquidation request
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of compliance checks</returns>
    [HttpGet("request/{liquidationRequestId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ComplianceCheckResponse>>> GetComplianceChecksForRequest(
        Guid liquidationRequestId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving compliance checks for liquidation request {RequestId}", liquidationRequestId);

            var result = await _complianceService.GetComplianceChecksForRequestAsync(liquidationRequestId, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} compliance checks for liquidation request {RequestId}", 
                result.Count(), liquidationRequestId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance checks for liquidation request {RequestId}", liquidationRequestId);
            return StatusCode(500, "An error occurred while retrieving compliance checks");
        }
    }

    /// <summary>
    /// Override a compliance check result (admin function)
    /// </summary>
    /// <param name="id">Compliance check ID</param>
    /// <param name="newResult">New compliance result</param>
    /// <param name="reason">Override reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated compliance check</returns>
    [HttpPut("{id:guid}/override")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ComplianceCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ComplianceCheckResponse>> OverrideComplianceCheck(
        Guid id,
        [FromQuery] string newResult,
        [FromQuery] string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Overriding compliance check {CheckId} to result {NewResult} by admin {AdminId}", 
                id, newResult, currentUserId);

            if (string.IsNullOrWhiteSpace(newResult))
            {
                return BadRequest("New result is required");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest("Override reason is required");
            }

            var result = await _complianceService.OverrideComplianceCheckAsync(
                id, newResult, reason, currentUserId, cancellationToken);
            
            _logger.LogInformation("Compliance check {CheckId} overridden successfully", id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for compliance check override");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error overriding compliance check {CheckId}", id);
            return StatusCode(500, "An error occurred while overriding the compliance check");
        }
    }

    /// <summary>
    /// Check if a liquidation request passes all compliance requirements
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Overall compliance approval status</returns>
    [HttpGet("approval/{liquidationRequestId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> IsComplianceApproved(
        Guid liquidationRequestId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking compliance approval status for liquidation request {RequestId}", liquidationRequestId);

            var result = await _complianceService.IsComplianceApprovedAsync(liquidationRequestId, cancellationToken);
            
            _logger.LogInformation("Compliance approval status for liquidation request {RequestId}: {IsApproved}", 
                liquidationRequestId, result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking compliance approval for liquidation request {RequestId}", liquidationRequestId);
            return StatusCode(500, "An error occurred while checking compliance approval");
        }
    }

    /// <summary>
    /// Get compliance statistics for reporting
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance statistics</returns>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetComplianceStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving compliance statistics from {FromDate} to {ToDate}", fromDate, toDate);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var stats = await _complianceService.GetComplianceStatisticsAsync(fromDate, toDate, cancellationToken);
            
            _logger.LogInformation("Retrieved compliance statistics successfully");
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance statistics");
            return StatusCode(500, "An error occurred while retrieving compliance statistics");
        }
    }

    /// <summary>
    /// Perform comprehensive compliance check (all checks combined)
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="assetSymbol">Asset being liquidated</param>
    /// <param name="amount">Amount being liquidated</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all compliance check results</returns>
    [HttpPost("comprehensive/{liquidationRequestId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ComplianceCheckResponse>>> PerformComprehensiveComplianceCheck(
        Guid liquidationRequestId,
        [FromQuery] string assetSymbol,
        [FromQuery] decimal amount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Performing comprehensive compliance check for liquidation request {RequestId} - Asset: {AssetSymbol}, Amount: {Amount}", 
                liquidationRequestId, assetSymbol, amount);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            var result = await _complianceService.PerformComprehensiveComplianceCheckAsync(
                liquidationRequestId, currentUserId, assetSymbol, amount, cancellationToken);
            
            _logger.LogInformation("Comprehensive compliance check completed for liquidation request {RequestId} with {Count} checks", 
                liquidationRequestId, result.Count());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for comprehensive compliance check");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing comprehensive compliance check for liquidation request {RequestId}", liquidationRequestId);
            return StatusCode(500, "An error occurred while performing comprehensive compliance check");
        }
    }

    /// <summary>
    /// Get pending compliance checks requiring manual review
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated compliance checks requiring review</returns>
    [HttpGet("pending-reviews")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaginatedResponse<ComplianceCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<ComplianceCheckResponse>>> GetPendingReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving pending compliance reviews - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            if (pageSize > 100)
            {
                return BadRequest("Maximum page size is 100");
            }

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            var result = await _complianceService.GetPendingReviewsAsync(page, pageSize, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} pending compliance reviews (Page {Page} of {TotalPages})", 
                result.Items.Count(), page, result.TotalPages);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for pending compliance reviews");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending compliance reviews");
            return StatusCode(500, "An error occurred while retrieving pending compliance reviews");
        }
    }

    #region Private Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    #endregion
}
