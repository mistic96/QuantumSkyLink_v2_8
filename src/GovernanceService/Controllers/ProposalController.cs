using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GovernanceService.Services.Interfaces;
using GovernanceService.Models.Requests;
using GovernanceService.Models.Responses;

namespace GovernanceService.Controllers;

/// <summary>
/// Controller for proposal lifecycle management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProposalController : ControllerBase
{
    private readonly IGovernanceService _governanceService;
    private readonly ILogger<ProposalController> _logger;

    public ProposalController(
        IGovernanceService governanceService,
        ILogger<ProposalController> logger)
    {
        _governanceService = governanceService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new governance proposal
    /// </summary>
    /// <param name="request">Proposal creation request</param>
    /// <returns>Created proposal details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProposalResponse>> CreateProposal(
        [FromBody] CreateProposalRequest request)
    {
        try
        {
            _logger.LogInformation("Creating governance proposal: {Title}, Type: {Type}, Creator: {CreatorId}", 
                request.Title, request.Type, request.CreatorId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate user can create proposal
            var canCreate = await _governanceService.CanUserCreateProposalAsync(request.CreatorId, request.Type);
            if (!canCreate)
            {
                return Forbid($"User {request.CreatorId} is not authorized to create {request.Type} proposals");
            }

            // Validate proposal request
            var isValid = await _governanceService.ValidateProposalAsync(request);
            if (!isValid)
            {
                return BadRequest("Proposal validation failed. Please check proposal requirements.");
            }

            var result = await _governanceService.CreateProposalAsync(request);
            
            _logger.LogInformation("Governance proposal created successfully: {ProposalId}", result.Id);
            return CreatedAtAction(nameof(GetProposal), new { proposalId = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid proposal creation request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Proposal creation failed: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized proposal creation attempt: {CreatorId}", request.CreatorId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating governance proposal: {Request}", request);
            return StatusCode(500, "An error occurred while creating the governance proposal");
        }
    }

    /// <summary>
    /// Get governance proposal by ID
    /// </summary>
    /// <param name="proposalId">Proposal ID</param>
    /// <returns>Proposal details with voting statistics</returns>
    [HttpGet("{proposalId}")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProposalResponse>> GetProposal(
        [FromRoute] Guid proposalId)
    {
        try
        {
            _logger.LogInformation("Getting governance proposal: {ProposalId}", proposalId);

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            var proposal = await _governanceService.GetProposalAsync(proposalId);
            
            if (proposal == null)
            {
                return NotFound($"Governance proposal not found: {proposalId}");
            }

            return Ok(proposal);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid proposal request: {ProposalId}", proposalId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting governance proposal: {ProposalId}", proposalId);
            return StatusCode(500, "An error occurred while retrieving the governance proposal");
        }
    }

    /// <summary>
    /// Get governance proposals with filtering and pagination
    /// </summary>
    /// <param name="type">Optional proposal type filter</param>
    /// <param name="status">Optional proposal status filter</param>
    /// <param name="creatorId">Optional creator ID filter</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field (default: CreatedAt)</param>
    /// <param name="sortDescending">Sort direction (default: true)</param>
    /// <returns>Paginated list of governance proposals</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProposalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<ProposalResponse>>> GetProposals(
        [FromQuery] string? type = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? creatorId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true)
    {
        try
        {
            _logger.LogInformation("Getting governance proposals - Type: {Type}, Status: {Status}, Creator: {CreatorId}, Page: {Page}, Size: {PageSize}", 
                type, status, creatorId, page, pageSize);

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var request = new GetProposalsRequest
            {
                Type = !string.IsNullOrEmpty(type) && Enum.TryParse<Data.Entities.ProposalType>(type, true, out var proposalType) ? proposalType : null,
                Status = !string.IsNullOrEmpty(status) && Enum.TryParse<Data.Entities.ProposalStatus>(status, true, out var proposalStatus) ? proposalStatus : null,
                CreatorId = creatorId,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy ?? "CreatedAt",
                SortDescending = sortDescending
            };

            var proposals = await _governanceService.GetProposalsAsync(request);
            return Ok(proposals);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid proposals request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting governance proposals");
            return StatusCode(500, "An error occurred while retrieving governance proposals");
        }
    }

    /// <summary>
    /// Update governance proposal
    /// </summary>
    /// <param name="proposalId">Proposal ID</param>
    /// <param name="request">Proposal update request</param>
    /// <returns>Updated proposal details</returns>
    [HttpPut("{proposalId}")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProposalResponse>> UpdateProposal(
        [FromRoute] Guid proposalId,
        [FromBody] UpdateProposalRequest request)
    {
        try
        {
            _logger.LogInformation("Updating governance proposal: {ProposalId}", proposalId);

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _governanceService.UpdateProposalAsync(proposalId, request);
            
            if (result == null)
            {
                return NotFound($"Governance proposal not found: {proposalId}");
            }

            _logger.LogInformation("Governance proposal updated successfully: {ProposalId}", proposalId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid proposal update request: {ProposalId}, {Request}", proposalId, request);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Proposal update failed: {ProposalId}", proposalId);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized proposal update attempt: {ProposalId}", proposalId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating governance proposal: {ProposalId}", proposalId);
            return StatusCode(500, "An error occurred while updating the governance proposal");
        }
    }

    /// <summary>
    /// Delete governance proposal
    /// </summary>
    /// <param name="proposalId">Proposal ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{proposalId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> DeleteProposal(
        [FromRoute] Guid proposalId)
    {
        try
        {
            _logger.LogInformation("Deleting governance proposal: {ProposalId}", proposalId);

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            var result = await _governanceService.DeleteProposalAsync(proposalId);
            
            if (!result)
            {
                return NotFound($"Governance proposal not found or cannot be deleted: {proposalId}");
            }

            _logger.LogInformation("Governance proposal deleted successfully: {ProposalId}", proposalId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid proposal deletion request: {ProposalId}", proposalId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Proposal deletion failed: {ProposalId}", proposalId);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized proposal deletion attempt: {ProposalId}", proposalId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting governance proposal: {ProposalId}", proposalId);
            return StatusCode(500, "An error occurred while deleting the governance proposal");
        }
    }

    /// <summary>
    /// Cancel governance proposal
    /// </summary>
    /// <param name="proposalId">Proposal ID</param>
    /// <param name="userId">User ID performing the cancellation</param>
    /// <returns>Cancelled proposal details</returns>
    [HttpPost("{proposalId}/cancel")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProposalResponse>> CancelProposal(
        [FromRoute] Guid proposalId,
        [FromBody] Guid userId)
    {
        try
        {
            _logger.LogInformation("Cancelling governance proposal: {ProposalId} by user: {UserId}", proposalId, userId);

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty");
            }

            var result = await _governanceService.CancelProposalAsync(proposalId, userId);
            
            if (result == null)
            {
                return NotFound($"Governance proposal not found: {proposalId}");
            }

            _logger.LogInformation("Governance proposal cancelled successfully: {ProposalId}", proposalId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid proposal cancellation request: {ProposalId}, {UserId}", proposalId, userId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Proposal cancellation failed: {ProposalId}", proposalId);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized proposal cancellation attempt: {ProposalId}, {UserId}", proposalId, userId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling governance proposal: {ProposalId}", proposalId);
            return StatusCode(500, "An error occurred while cancelling the governance proposal");
        }
    }

    /// <summary>
    /// Get active governance proposals
    /// </summary>
    /// <returns>List of active proposals</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<ProposalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ProposalResponse>>> GetActiveProposals()
    {
        try
        {
            _logger.LogInformation("Getting active governance proposals");

            var proposals = await _governanceService.GetActiveProposalsAsync();
            return Ok(proposals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active governance proposals");
            return StatusCode(500, "An error occurred while retrieving active governance proposals");
        }
    }

    /// <summary>
    /// Get governance proposals requiring action
    /// </summary>
    /// <returns>List of proposals requiring action</returns>
    [HttpGet("requiring-action")]
    [ProducesResponseType(typeof(List<ProposalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ProposalResponse>>> GetProposalsRequiringAction()
    {
        try
        {
            _logger.LogInformation("Getting governance proposals requiring action");

            var proposals = await _governanceService.GetProposalsRequiringActionAsync();
            return Ok(proposals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting governance proposals requiring action");
            return StatusCode(500, "An error occurred while retrieving governance proposals requiring action");
        }
    }
}
