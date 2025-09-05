using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GovernanceService.Services.Interfaces;
using GovernanceService.Models.Requests;
using GovernanceService.Models.Responses;

namespace GovernanceService.Controllers;

/// <summary>
/// Controller for voting operations and delegation management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class VotingController : ControllerBase
{
    private readonly IGovernanceService _governanceService;
    private readonly ILogger<VotingController> _logger;

    public VotingController(
        IGovernanceService governanceService,
        ILogger<VotingController> logger)
    {
        _governanceService = governanceService;
        _logger = logger;
    }

    /// <summary>
    /// Cast a vote on a governance proposal
    /// </summary>
    /// <param name="request">Vote casting request</param>
    /// <returns>Vote details</returns>
    [HttpPost("vote")]
    [ProducesResponseType(typeof(VoteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VoteResponse>> CastVote(
        [FromBody] CastVoteRequest request)
    {
        try
        {
            _logger.LogInformation("Casting vote: Proposal {ProposalId}, Voter {VoterId}, Choice {Choice}", 
                request.ProposalId, request.VoterId, request.Choice);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate user can vote on this proposal
            var canVote = await _governanceService.CanUserVoteAsync(request.VoterId, request.ProposalId);
            if (!canVote)
            {
                return Forbid($"User {request.VoterId} is not authorized to vote on proposal {request.ProposalId}");
            }

            // Check if proposal is active
            var isActive = await _governanceService.IsProposalActiveAsync(request.ProposalId);
            if (!isActive)
            {
                return BadRequest("Proposal is not active for voting");
            }

            // Check if user has already voted
            var hasVoted = await _governanceService.HasUserVotedAsync(request.ProposalId, request.VoterId);
            if (hasVoted)
            {
                return BadRequest("User has already voted on this proposal");
            }

            var result = await _governanceService.CastVoteAsync(request);
            
            _logger.LogInformation("Vote cast successfully: {VoteId}", result.Id);
            return CreatedAtAction(nameof(GetVote), new { proposalId = request.ProposalId, voterId = request.VoterId }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid vote casting request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Vote casting failed: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized vote casting attempt: {VoterId}", request.VoterId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error casting vote: {Request}", request);
            return StatusCode(500, "An error occurred while casting the vote");
        }
    }

    /// <summary>
    /// Get a specific vote by proposal and voter
    /// </summary>
    /// <param name="proposalId">Proposal ID</param>
    /// <param name="voterId">Voter ID</param>
    /// <returns>Vote details</returns>
    [HttpGet("vote/{proposalId}/{voterId}")]
    [ProducesResponseType(typeof(VoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VoteResponse>> GetVote(
        [FromRoute] Guid proposalId,
        [FromRoute] Guid voterId)
    {
        try
        {
            _logger.LogInformation("Getting vote: Proposal {ProposalId}, Voter {VoterId}", proposalId, voterId);

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            if (voterId == Guid.Empty)
            {
                return BadRequest("Voter ID cannot be empty");
            }

            var vote = await _governanceService.GetVoteAsync(proposalId, voterId);
            
            if (vote == null)
            {
                return NotFound($"Vote not found for proposal {proposalId} and voter {voterId}");
            }

            return Ok(vote);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid vote request: Proposal {ProposalId}, Voter {VoterId}", proposalId, voterId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vote: Proposal {ProposalId}, Voter {VoterId}", proposalId, voterId);
            return StatusCode(500, "An error occurred while retrieving the vote");
        }
    }

    /// <summary>
    /// Get votes for a proposal with filtering and pagination
    /// </summary>
    /// <param name="proposalId">Proposal ID</param>
    /// <param name="choice">Optional vote choice filter</param>
    /// <param name="voterId">Optional voter ID filter</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 50, max: 100)</param>
    /// <param name="sortBy">Sort field (default: CastAt)</param>
    /// <param name="sortDescending">Sort direction (default: true)</param>
    /// <returns>Paginated list of votes</returns>
    [HttpGet("votes")]
    [ProducesResponseType(typeof(PagedResponse<VoteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<VoteResponse>>> GetVotes(
        [FromQuery] Guid proposalId,
        [FromQuery] string? choice = null,
        [FromQuery] Guid? voterId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? sortBy = "CastAt",
        [FromQuery] bool sortDescending = true)
    {
        try
        {
            _logger.LogInformation("Getting votes for proposal: {ProposalId}, Choice: {Choice}, Voter: {VoterId}, Page: {Page}, Size: {PageSize}", 
                proposalId, choice, voterId, page, pageSize);

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var request = new GetVotesRequest
            {
                ProposalId = proposalId,
                Choice = !string.IsNullOrEmpty(choice) && Enum.TryParse<Data.Entities.VoteChoice>(choice, true, out var voteChoice) ? voteChoice : null,
                VoterId = voterId,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy ?? "CastAt",
                SortDescending = sortDescending
            };

            var votes = await _governanceService.GetVotesAsync(request);
            return Ok(votes);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid votes request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting votes");
            return StatusCode(500, "An error occurred while retrieving votes");
        }
    }

    /// <summary>
    /// Get user's voting power
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="proposalId">Optional proposal ID for context-specific voting power</param>
    /// <returns>Voting power amount</returns>
    [HttpGet("power/{userId}")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<decimal>> GetVotingPower(
        [FromRoute] Guid userId,
        [FromQuery] Guid? proposalId = null)
    {
        try
        {
            _logger.LogInformation("Getting voting power for user: {UserId}, Proposal: {ProposalId}", userId, proposalId);

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty");
            }

            var votingPower = await _governanceService.CalculateVotingPowerAsync(userId, proposalId);
            return Ok(votingPower);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid voting power request: {UserId}", userId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voting power: {UserId}", userId);
            return StatusCode(500, "An error occurred while calculating voting power");
        }
    }

    /// <summary>
    /// Get user's effective voting power for a specific proposal
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="proposalId">Proposal ID</param>
    /// <returns>Effective voting power amount</returns>
    [HttpGet("effective-power/{userId}/{proposalId}")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<decimal>> GetEffectiveVotingPower(
        [FromRoute] Guid userId,
        [FromRoute] Guid proposalId)
    {
        try
        {
            _logger.LogInformation("Getting effective voting power for user: {UserId}, Proposal: {ProposalId}", userId, proposalId);

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty");
            }

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            var effectivePower = await _governanceService.CalculateEffectiveVotingPowerAsync(userId, proposalId);
            return Ok(effectivePower);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid effective voting power request: {UserId}, {ProposalId}", userId, proposalId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting effective voting power: {UserId}, {ProposalId}", userId, proposalId);
            return StatusCode(500, "An error occurred while calculating effective voting power");
        }
    }

    /// <summary>
    /// Delegate voting power to another user
    /// </summary>
    /// <param name="request">Delegation request</param>
    /// <returns>Delegation details</returns>
    [HttpPost("delegate")]
    [ProducesResponseType(typeof(VotingDelegationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VotingDelegationResponse>> DelegateVotingPower(
        [FromBody] DelegateVoteRequest request)
    {
        try
        {
            _logger.LogInformation("Delegating voting power: Delegator {DelegatorId} to Delegate {DelegateId}, Type: {SpecificType}", 
                request.DelegatorId, request.DelegateId, request.SpecificType);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.DelegatorId == request.DelegateId)
            {
                return BadRequest("Cannot delegate voting power to yourself");
            }

            var result = await _governanceService.DelegateVotingPowerAsync(request);
            
            _logger.LogInformation("Voting power delegated successfully: {DelegationId}", result.Id);
            return CreatedAtAction(nameof(GetDelegations), new { delegatorId = request.DelegatorId }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid delegation request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Delegation failed: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized delegation attempt: {DelegatorId}", request.DelegatorId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegating voting power: {Request}", request);
            return StatusCode(500, "An error occurred while delegating voting power");
        }
    }

    /// <summary>
    /// Revoke voting delegation
    /// </summary>
    /// <param name="delegationId">Delegation ID</param>
    /// <param name="userId">User ID performing the revocation</param>
    /// <param name="reason">Optional revocation reason</param>
    /// <returns>Success status</returns>
    [HttpDelete("delegate/{delegationId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> RevokeDelegation(
        [FromRoute] Guid delegationId,
        [FromBody] Guid userId,
        [FromQuery] string? reason = null)
    {
        try
        {
            _logger.LogInformation("Revoking delegation: {DelegationId} by user: {UserId}, Reason: {Reason}", 
                delegationId, userId, reason);

            if (delegationId == Guid.Empty)
            {
                return BadRequest("Delegation ID cannot be empty");
            }

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty");
            }

            var result = await _governanceService.RevokeVotingDelegationAsync(delegationId, userId, reason);
            
            if (!result)
            {
                return NotFound($"Delegation not found or cannot be revoked: {delegationId}");
            }

            _logger.LogInformation("Delegation revoked successfully: {DelegationId}", delegationId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid delegation revocation request: {DelegationId}, {UserId}", delegationId, userId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Delegation revocation failed: {DelegationId}", delegationId);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized delegation revocation attempt: {DelegationId}, {UserId}", delegationId, userId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking delegation: {DelegationId}", delegationId);
            return StatusCode(500, "An error occurred while revoking the delegation");
        }
    }

    /// <summary>
    /// Get delegations with filtering and pagination
    /// </summary>
    /// <param name="delegatorId">Optional delegator ID filter</param>
    /// <param name="delegateId">Optional delegate ID filter</param>
    /// <param name="specificType">Optional proposal type filter</param>
    /// <param name="isActive">Optional active status filter</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of delegations</returns>
    [HttpGet("delegations")]
    [ProducesResponseType(typeof(PagedResponse<VotingDelegationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<VotingDelegationResponse>>> GetDelegations(
        [FromQuery] Guid? delegatorId = null,
        [FromQuery] Guid? delegateId = null,
        [FromQuery] string? specificType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Getting delegations - Delegator: {DelegatorId}, Delegate: {DelegateId}, Type: {SpecificType}, Active: {IsActive}, Page: {Page}, Size: {PageSize}", 
                delegatorId, delegateId, specificType, isActive, page, pageSize);

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var request = new GetDelegationsRequest
            {
                DelegatorId = delegatorId,
                DelegateId = delegateId,
                SpecificType = !string.IsNullOrEmpty(specificType) && Enum.TryParse<Data.Entities.ProposalType>(specificType, true, out var proposalType) ? proposalType : null,
                IsActive = isActive,
                Page = page,
                PageSize = pageSize
            };

            var delegations = await _governanceService.GetDelegationsAsync(request);
            return Ok(delegations);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid delegations request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delegations");
            return StatusCode(500, "An error occurred while retrieving delegations");
        }
    }

    /// <summary>
    /// Get user's delegations (delegations made by the user)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="isActive">Optional active status filter</param>
    /// <returns>List of user's delegations</returns>
    [HttpGet("user-delegations/{userId}")]
    [ProducesResponseType(typeof(List<VotingDelegationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<VotingDelegationResponse>>> GetUserDelegations(
        [FromRoute] Guid userId,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            _logger.LogInformation("Getting user delegations: {UserId}, Active: {IsActive}", userId, isActive);

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty");
            }

            var delegations = await _governanceService.GetUserDelegationsAsync(userId, isActive);
            return Ok(delegations);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid user delegations request: {UserId}", userId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user delegations: {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user delegations");
        }
    }

    /// <summary>
    /// Get delegations to a user (delegations received by the user)
    /// </summary>
    /// <param name="delegateId">Delegate user ID</param>
    /// <param name="isActive">Optional active status filter</param>
    /// <returns>List of delegations to the user</returns>
    [HttpGet("delegations-to/{delegateId}")]
    [ProducesResponseType(typeof(List<VotingDelegationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<VotingDelegationResponse>>> GetDelegationsToUser(
        [FromRoute] Guid delegateId,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            _logger.LogInformation("Getting delegations to user: {DelegateId}, Active: {IsActive}", delegateId, isActive);

            if (delegateId == Guid.Empty)
            {
                return BadRequest("Delegate ID cannot be empty");
            }

            var delegations = await _governanceService.GetDelegationsToUserAsync(delegateId, isActive);
            return Ok(delegations);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid delegations to user request: {DelegateId}", delegateId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delegations to user: {DelegateId}", delegateId);
            return StatusCode(500, "An error occurred while retrieving delegations to user");
        }
    }
}
