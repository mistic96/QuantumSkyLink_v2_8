using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LiquidationService.Models.Requests;
using LiquidationService.Models.Responses;
using LiquidationService.Services.Interfaces;
using System.Security.Claims;

namespace LiquidationService.Controllers;

/// <summary>
/// Controller for managing liquidity providers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LiquidityProviderController : ControllerBase
{
    private readonly ILiquidityProviderService _liquidityProviderService;
    private readonly ILogger<LiquidityProviderController> _logger;

    public LiquidityProviderController(
        ILiquidityProviderService liquidityProviderService,
        ILogger<LiquidityProviderController> logger)
    {
        _liquidityProviderService = liquidityProviderService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new liquidity provider
    /// </summary>
    /// <param name="request">Provider registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registered liquidity provider</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LiquidityProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidityProviderResponse>> RegisterLiquidityProvider(
        [FromBody] RegisterLiquidityProviderModel request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Registering liquidity provider for user {UserId} - Name: {ProviderName}", 
                currentUserId, request.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _liquidityProviderService.RegisterLiquidityProviderAsync(currentUserId, request, cancellationToken);
            
            _logger.LogInformation("Liquidity provider registered successfully with ID {ProviderId}", result.Id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for liquidity provider registration");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering liquidity provider");
            return StatusCode(500, "An error occurred while registering the liquidity provider");
        }
    }

    /// <summary>
    /// Get a specific liquidity provider by ID
    /// </summary>
    /// <param name="id">Liquidity provider ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liquidity provider details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LiquidityProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidityProviderResponse>> GetLiquidityProvider(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Retrieving liquidity provider {ProviderId} for user {UserId}", id, currentUserId);

            var result = await _liquidityProviderService.GetLiquidityProviderAsync(id, cancellationToken);
            if (result == null)
            {
                _logger.LogWarning("Liquidity provider {ProviderId} not found", id);
                return NotFound($"Liquidity provider with ID {id} not found");
            }

            // Additional validation - users can only view their own providers unless admin
            if (!IsAdmin() && result.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to access liquidity provider {ProviderId} belonging to user {OwnerId}", 
                    currentUserId, id, result.UserId);
                return Forbid("You can only access your own liquidity providers");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving liquidity provider {ProviderId}", id);
            return StatusCode(500, "An error occurred while retrieving the liquidity provider");
        }
    }

    /// <summary>
    /// Get all liquidity providers with filtering
    /// </summary>
    /// <param name="status">Filter by status</param>
    /// <param name="country">Filter by country</param>
    /// <param name="assetSymbol">Filter by supported asset</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of liquidity providers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<LiquidityProviderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<LiquidityProviderResponse>>> GetLiquidityProviders(
        [FromQuery] string? status = null,
        [FromQuery] string? country = null,
        [FromQuery] string? assetSymbol = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Retrieving liquidity providers for user {UserId} - Page: {Page}, PageSize: {PageSize}", 
                currentUserId, page, pageSize);

            if (pageSize > 100)
            {
                return BadRequest("Maximum page size is 100");
            }

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            var result = await _liquidityProviderService.GetLiquidityProvidersAsync(
                status, country, assetSymbol, page, pageSize, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} liquidity providers (Page {Page} of {TotalPages})", 
                result.Items.Count(), page, result.TotalPages);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for getting liquidity providers");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving liquidity providers");
            return StatusCode(500, "An error occurred while retrieving liquidity providers");
        }
    }

    /// <summary>
    /// Update liquidity provider information
    /// </summary>
    /// <param name="id">Liquidity provider ID</param>
    /// <param name="request">Update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidity provider</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LiquidityProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidityProviderResponse>> UpdateLiquidityProvider(
        Guid id,
        [FromBody] UpdateLiquidityProviderModel request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Updating liquidity provider {ProviderId} for user {UserId}", id, currentUserId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // First check if provider exists and user has access
            var existingProvider = await _liquidityProviderService.GetLiquidityProviderAsync(id, cancellationToken);
            if (existingProvider == null)
            {
                _logger.LogWarning("Liquidity provider {ProviderId} not found for update", id);
                return NotFound($"Liquidity provider with ID {id} not found");
            }

            // Validate user access - users can only update their own providers unless admin
            if (!IsAdmin() && existingProvider.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to update liquidity provider {ProviderId} belonging to user {OwnerId}", 
                    currentUserId, id, existingProvider.UserId);
                return Forbid("You can only update your own liquidity providers");
            }

            var result = await _liquidityProviderService.UpdateLiquidityProviderAsync(id, request, cancellationToken);
            
            _logger.LogInformation("Liquidity provider {ProviderId} updated successfully", id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating liquidity provider");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating liquidity provider {ProviderId}", id);
            return StatusCode(500, "An error occurred while updating the liquidity provider");
        }
    }

    /// <summary>
    /// Approve a liquidity provider
    /// </summary>
    /// <param name="id">Liquidity provider ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidity provider</returns>
    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(LiquidityProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidityProviderResponse>> ApproveLiquidityProvider(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Approving liquidity provider {ProviderId}", id);

            var result = await _liquidityProviderService.ApproveLiquidityProviderAsync(id, cancellationToken);
            
            _logger.LogInformation("Liquidity provider {ProviderId} approved successfully", id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for approving liquidity provider");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving liquidity provider {ProviderId}", id);
            return StatusCode(500, "An error occurred while approving the liquidity provider");
        }
    }

    /// <summary>
    /// Suspend a liquidity provider
    /// </summary>
    /// <param name="id">Liquidity provider ID</param>
    /// <param name="reason">Suspension reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidity provider</returns>
    [HttpPut("{id:guid}/suspend")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(LiquidityProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidityProviderResponse>> SuspendLiquidityProvider(
        Guid id,
        [FromQuery] string reason = "Administrative suspension",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Suspending liquidity provider {ProviderId} with reason: {Reason}", id, reason);

            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest("Suspension reason is required");
            }

            var result = await _liquidityProviderService.SuspendLiquidityProviderAsync(id, reason, cancellationToken);
            
            _logger.LogInformation("Liquidity provider {ProviderId} suspended successfully", id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for suspending liquidity provider");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending liquidity provider {ProviderId}", id);
            return StatusCode(500, "An error occurred while suspending the liquidity provider");
        }
    }

    /// <summary>
    /// Find the best liquidity provider for a liquidation request
    /// </summary>
    /// <param name="assetSymbol">Asset to liquidate</param>
    /// <param name="outputSymbol">Desired output currency</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Best matching liquidity provider</returns>
    [HttpGet("best")]
    [ProducesResponseType(typeof(LiquidityProviderSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidityProviderSummaryResponse>> FindBestLiquidityProvider(
        [FromQuery] string assetSymbol,
        [FromQuery] string outputSymbol,
        [FromQuery] decimal amount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Finding best liquidity provider for {AssetSymbol} {Amount} to {OutputSymbol}", 
                assetSymbol, amount, outputSymbol);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            var result = await _liquidityProviderService.FindBestLiquidityProviderAsync(
                assetSymbol, outputSymbol, amount, cancellationToken);
            
            _logger.LogInformation("Best liquidity provider found successfully");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for finding best liquidity provider");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding best liquidity provider");
            return StatusCode(500, "An error occurred while finding the best liquidity provider");
        }
    }

    /// <summary>
    /// Get liquidity provider statistics
    /// </summary>
    /// <param name="id">Liquidity provider ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Provider statistics</returns>
    [HttpGet("{id:guid}/statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetLiquidityProviderStatistics(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Retrieving statistics for liquidity provider {ProviderId}", id);

            // First check if provider exists and user has access
            var provider = await _liquidityProviderService.GetLiquidityProviderAsync(id, cancellationToken);
            if (provider == null)
            {
                _logger.LogWarning("Liquidity provider {ProviderId} not found for statistics", id);
                return NotFound($"Liquidity provider with ID {id} not found");
            }

            // Validate user access - users can only view their own provider stats unless admin
            if (!IsAdmin() && provider.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to access statistics for liquidity provider {ProviderId} belonging to user {OwnerId}", 
                    currentUserId, id, provider.UserId);
                return Forbid("You can only access statistics for your own liquidity providers");
            }

            var stats = await _liquidityProviderService.GetLiquidityProviderStatisticsAsync(id, cancellationToken);
            
            _logger.LogInformation("Retrieved statistics for liquidity provider {ProviderId} successfully", id);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for liquidity provider {ProviderId}", id);
            return StatusCode(500, "An error occurred while retrieving liquidity provider statistics");
        }
    }

    /// <summary>
    /// Update liquidity provider availability
    /// </summary>
    /// <param name="id">Liquidity provider ID</param>
    /// <param name="isAvailable">Availability status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidity provider</returns>
    [HttpPut("{id:guid}/availability")]
    [ProducesResponseType(typeof(LiquidityProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidityProviderResponse>> UpdateAvailability(
        Guid id,
        [FromQuery] bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Updating availability for liquidity provider {ProviderId} to {IsAvailable}", id, isAvailable);

            // First check if provider exists and user has access
            var existingProvider = await _liquidityProviderService.GetLiquidityProviderAsync(id, cancellationToken);
            if (existingProvider == null)
            {
                _logger.LogWarning("Liquidity provider {ProviderId} not found for availability update", id);
                return NotFound($"Liquidity provider with ID {id} not found");
            }

            // Validate user access - users can only update their own providers unless admin
            if (!IsAdmin() && existingProvider.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to update availability for liquidity provider {ProviderId} belonging to user {OwnerId}", 
                    currentUserId, id, existingProvider.UserId);
                return Forbid("You can only update availability for your own liquidity providers");
            }

            var result = await _liquidityProviderService.UpdateAvailabilityAsync(id, isAvailable, cancellationToken);
            
            _logger.LogInformation("Availability updated successfully for liquidity provider {ProviderId}", id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating liquidity provider availability");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating availability for liquidity provider {ProviderId}", id);
            return StatusCode(500, "An error occurred while updating liquidity provider availability");
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
