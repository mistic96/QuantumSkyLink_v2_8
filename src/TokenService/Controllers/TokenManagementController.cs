using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TokenService.Models.Requests;
using TokenService.Models.Responses;
using TokenService.Services.Interfaces;

namespace TokenService.Controllers;

[Authorize]
[ApiController]
[Route("api/tokens")]
public class TokenManagementController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<TokenManagementController> _logger;

    public TokenManagementController(
        ITokenService tokenService,
        ILogger<TokenManagementController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Create a token from an approved submission
    /// </summary>
    /// <param name="submissionId">The unique identifier of the approved token submission</param>
    /// <returns>Token creation response with QuantumLedger integration details</returns>
    [HttpPost("{submissionId}/create")]
    public async Task<ActionResult<TokenCreationResponse>> CreateToken(Guid submissionId)
    {
        try
        {
            // First verify the submission exists and belongs to the current user
            var submission = await _tokenService.GetSubmissionAsync(submissionId);
            
            if (submission == null)
            {
                return NotFound(new { error = "Token submission not found" });
            }
            
            var currentUserId = GetCurrentUserId();
            if (submission.CreatorId != currentUserId)
            {
                return Forbid("You can only create tokens from your own approved submissions");
            }
            
            // Check if submission is approved
            if (submission.ApprovalStatus != "Approved")
            {
                return BadRequest(new { error = "Only approved submissions can be used to create tokens" });
            }
            
            // Check if token already exists for this submission
            if (submission.TokenId.HasValue)
            {
                return BadRequest(new { error = "Token has already been created for this submission" });
            }
            
            var response = await _tokenService.CreateTokenAsync(submissionId);
            
            _logger.LogInformation("Token created successfully from submission {SubmissionId} for user {UserId} with token ID {TokenId}", 
                submissionId, currentUserId, response.TokenId);
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid token creation request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized token creation attempt: {Error}", ex.Message);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating token from submission {SubmissionId}", submissionId);
            return StatusCode(500, new { error = "Internal server error occurred while creating token" });
        }
    }

    /// <summary>
    /// Get details of a specific token
    /// </summary>
    /// <param name="tokenId">The unique identifier of the token</param>
    /// <returns>Detailed token information including QuantumLedger integration</returns>
    [HttpGet("{tokenId}")]
    public async Task<ActionResult<TokenResponse>> GetToken(Guid tokenId)
    {
        try
        {
            var token = await _tokenService.GetTokenAsync(tokenId);
            
            if (token == null)
            {
                return NotFound(new { error = "Token not found" });
            }
            
            // Ensure user can only access their own tokens
            var currentUserId = GetCurrentUserId();
            if (token.CreatorId != currentUserId)
            {
                return Forbid("You can only access your own tokens");
            }
            
            return Ok(token);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token {TokenId}", tokenId);
            return StatusCode(500, new { error = "Internal server error occurred while retrieving token" });
        }
    }

    /// <summary>
    /// Get all tokens created by the current user with optional filtering
    /// </summary>
    /// <param name="status">Filter by token status (Active, Suspended, Burned)</param>
    /// <param name="assetType">Filter by asset type (RealEstate, Commodity, Security, etc.)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of user's tokens</returns>
    [HttpGet]
    public async Task<ActionResult<TokenListResponse>> GetUserTokens(
        [FromQuery] string? status = null,
        [FromQuery] string? assetType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;
            
            var currentUserId = GetCurrentUserId();
            
            var tokens = await _tokenService.GetTokensByCreatorAsync(currentUserId);
            
            // Apply filters
            var filteredTokens = tokens.AsEnumerable();
            
            if (!string.IsNullOrEmpty(status))
            {
                filteredTokens = filteredTokens.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!string.IsNullOrEmpty(assetType))
            {
                filteredTokens = filteredTokens.Where(t => t.AssetType?.Equals(assetType, StringComparison.OrdinalIgnoreCase) == true);
            }
            
            // Apply pagination
            var totalCount = filteredTokens.Count();
            var paginatedTokens = filteredTokens
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var response = new TokenListResponse
            {
                Tokens = paginatedTokens,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tokens for user");
            return StatusCode(500, new { error = "Internal server error occurred while retrieving tokens" });
        }
    }

    /// <summary>
    /// Update the status of a token (suspend/reactivate)
    /// </summary>
    /// <param name="tokenId">The unique identifier of the token</param>
    /// <param name="request">Token status update request</param>
    /// <returns>Success confirmation or error message</returns>
    [HttpPut("{tokenId}/status")]
    public async Task<ActionResult> UpdateTokenStatus(Guid tokenId, [FromBody] TokenStatusUpdateRequest request)
    {
        try
        {
            // First verify the token exists and belongs to the current user
            var token = await _tokenService.GetTokenAsync(tokenId);
            
            if (token == null)
            {
                return NotFound(new { error = "Token not found" });
            }
            
            var currentUserId = GetCurrentUserId();
            if (token.CreatorId != currentUserId)
            {
                return Forbid("You can only update the status of your own tokens");
            }
            
            bool result = false;
            string action = "";
            
            switch (request.Status.ToLower())
            {
                case "suspended":
                    result = await _tokenService.SuspendTokenAsync(tokenId, request.Reason ?? "User requested suspension");
                    action = "suspended";
                    break;
                case "active":
                    result = await _tokenService.ReactivateTokenAsync(tokenId);
                    action = "reactivated";
                    break;
                default:
                    return BadRequest(new { error = "Invalid status. Allowed values: Suspended, Active" });
            }
            
            if (result)
            {
                _logger.LogInformation("Token {TokenId} {Action} by user {UserId}", tokenId, action, currentUserId);
                return Ok(new { message = $"Token {action} successfully" });
            }
            
            return BadRequest(new { error = $"Failed to {action.ToLower()} token" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating token status for token {TokenId}", tokenId);
            return StatusCode(500, new { error = "Internal server error occurred while updating token status" });
        }
    }

    /// <summary>
    /// Burn a specified amount of tokens
    /// </summary>
    /// <param name="tokenId">The unique identifier of the token</param>
    /// <param name="request">Token burn request with amount and reason</param>
    /// <returns>Success confirmation or error message</returns>
    [HttpPost("{tokenId}/burn")]
    public async Task<ActionResult> BurnToken(Guid tokenId, [FromBody] BurnTokenRequest request)
    {
        try
        {
            // First verify the token exists and belongs to the current user
            var token = await _tokenService.GetTokenAsync(tokenId);
            
            if (token == null)
            {
                return NotFound(new { error = "Token not found" });
            }
            
            var currentUserId = GetCurrentUserId();
            if (token.CreatorId != currentUserId)
            {
                return Forbid("You can only burn your own tokens");
            }
            
            // Validate burn amount
            if (request.Amount <= 0)
            {
                return BadRequest(new { error = "Burn amount must be greater than zero" });
            }
            
            if (request.Amount > token.TotalSupply)
            {
                return BadRequest(new { error = "Cannot burn more tokens than total supply" });
            }
            
            var result = await _tokenService.BurnTokenAsync(tokenId, request.Amount);
            
            if (result)
            {
                _logger.LogInformation("Burned {Amount} tokens from token {TokenId} by user {UserId}. Reason: {Reason}", 
                    request.Amount, tokenId, currentUserId, request.Reason);
                return Ok(new { message = $"Successfully burned {request.Amount} tokens", burnedAmount = request.Amount });
            }
            
            return BadRequest(new { error = "Failed to burn tokens" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error burning tokens for token {TokenId}", tokenId);
            return StatusCode(500, new { error = "Internal server error occurred while burning tokens" });
        }
    }

    /// <summary>
    /// Sync token data with QuantumLedger
    /// </summary>
    /// <param name="tokenId">The unique identifier of the token</param>
    /// <returns>Success confirmation or error message</returns>
    [HttpPost("{tokenId}/sync")]
    public async Task<ActionResult> SyncWithQuantumLedger(Guid tokenId)
    {
        try
        {
            // First verify the token exists and belongs to the current user
            var token = await _tokenService.GetTokenAsync(tokenId);
            
            if (token == null)
            {
                return NotFound(new { error = "Token not found" });
            }
            
            var currentUserId = GetCurrentUserId();
            if (token.CreatorId != currentUserId)
            {
                return Forbid("You can only sync your own tokens");
            }
            
            var result = await _tokenService.SyncWithQuantumLedgerAsync(tokenId);
            
            if (result)
            {
                _logger.LogInformation("Token {TokenId} synced with QuantumLedger by user {UserId}", tokenId, currentUserId);
                return Ok(new { message = "Token synced with QuantumLedger successfully" });
            }
            
            return BadRequest(new { error = "Failed to sync token with QuantumLedger" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing token {TokenId} with QuantumLedger", tokenId);
            return StatusCode(500, new { error = "Internal server error occurred while syncing with QuantumLedger" });
        }
    }

    /// <summary>
    /// Deploy token to multi-chain networks
    /// </summary>
    /// <param name="tokenId">The unique identifier of the token</param>
    /// <returns>Multi-chain deployment details</returns>
    [HttpPost("{tokenId}/deploy-multichain")]
    public async Task<ActionResult> DeployToMultiChain(Guid tokenId)
    {
        try
        {
            // First verify the token exists and belongs to the current user
            var token = await _tokenService.GetTokenAsync(tokenId);
            
            if (token == null)
            {
                return NotFound(new { error = "Token not found" });
            }
            
            var currentUserId = GetCurrentUserId();
            if (token.CreatorId != currentUserId)
            {
                return Forbid("You can only deploy your own tokens to multi-chain");
            }
            
            // Check if token is already deployed
            if (token.CrossChainEnabled)
            {
                return BadRequest(new { error = "Token is already deployed to multi-chain networks" });
            }
            
            var multiChainAssetName = await _tokenService.DeployToMultiChainAsync(tokenId);
            
            _logger.LogInformation("Token {TokenId} deployed to multi-chain by user {UserId}. Asset name: {AssetName}", 
                tokenId, currentUserId, multiChainAssetName);
            
            return Ok(new { 
                message = "Token deployed to multi-chain successfully", 
                multiChainAssetName = multiChainAssetName 
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying token {TokenId} to multi-chain", tokenId);
            return StatusCode(500, new { error = "Internal server error occurred while deploying to multi-chain" });
        }
    }

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value 
                         ?? User.FindFirst("user_id")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        
        return userId;
    }

    #endregion
}

// Additional request/response models for this controller
public class TokenStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty; // Suspended, Active
    public string? Reason { get; set; }
}

public class BurnTokenRequest
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class TokenListResponse
{
    public List<TokenResponse> Tokens { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
