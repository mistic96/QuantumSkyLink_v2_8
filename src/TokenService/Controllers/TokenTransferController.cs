using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TokenService.Models.Requests;
using TokenService.Models.Responses;
using TokenService.Services.Interfaces;

namespace TokenService.Controllers;

[Authorize]
[ApiController]
[Route("api/token-transfers")]
public class TokenTransferController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<TokenTransferController> _logger;

    public TokenTransferController(
        ITokenService tokenService,
        ILogger<TokenTransferController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Execute a token transfer between accounts
    /// </summary>
    /// <param name="request">Token transfer details including from/to accounts and amount</param>
    /// <returns>Token transfer response with transaction details</returns>
    [HttpPost]
    public async Task<ActionResult<TokenTransferResponse>> TransferToken([FromBody] TokenTransferRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Validate that the user owns the source account
            // This would typically involve checking with AccountService
            // For now, we'll assume the request is valid if the user is authenticated
            
            var response = await _tokenService.TransferTokenAsync(request);
            
            _logger.LogInformation("Token transfer initiated successfully by user {UserId} with transfer ID {TransferId}", 
                currentUserId, response.TransferId);
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid token transfer request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized token transfer attempt: {Error}", ex.Message);
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid token transfer operation: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogError(ex, "Error executing token transfer for user {UserId}", currentUserId);
            return StatusCode(500, new { error = "Internal server error occurred while executing transfer" });
        }
    }

    /// <summary>
    /// Get details of a specific token transfer
    /// </summary>
    /// <param name="transferId">The unique identifier of the token transfer</param>
    /// <returns>Detailed token transfer information including transaction status</returns>
    [HttpGet("{transferId}")]
    public async Task<ActionResult<TokenTransferDetailResponse>> GetTransfer(Guid transferId)
    {
        try
        {
            var transfer = await _tokenService.GetTransferAsync(transferId);
            
            if (transfer == null)
            {
                return NotFound(new { error = "Token transfer not found" });
            }
            
            var currentUserId = GetCurrentUserId();
            
            // User can access transfer if they are involved in it (sender or receiver)
            // This would typically involve checking account ownership
            // For now, we'll allow access to all authenticated users
            
            // Convert entity to detailed response
            var response = new TokenTransferDetailResponse
            {
                TransferId = transfer.Id,
                TokenId = transfer.TokenId,
                FromAccountId = transfer.FromAccountId,
                ToAccountId = transfer.ToAccountId,
                Amount = transfer.Amount,
                TransactionFee = transfer.TransactionFee,
                Status = transfer.Status,
                TransferType = "Transfer", // Default value since entity doesn't have this property
                Description = transfer.Description,
                CreatedAt = transfer.CreatedAt,
                CompletedAt = transfer.CompletedAt,
                FailureReason = transfer.FailureReason,
                ExternalTransactionHash = transfer.ExternalTransactionHash,
                QuantumLedgerTransactionId = transfer.QuantumLedgerTransactionId,
                MultiChainTransactionId = transfer.MultiChainTransactionId
            };
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token transfer {TransferId}", transferId);
            return StatusCode(500, new { error = "Internal server error occurred while retrieving transfer" });
        }
    }

    /// <summary>
    /// Get transfer history for a specific account with optional filtering
    /// </summary>
    /// <param name="accountId">The account ID to get transfer history for</param>
    /// <param name="tokenId">Optional: Filter by specific token</param>
    /// <param name="status">Optional: Filter by transfer status (Pending, Completed, Failed)</param>
    /// <param name="transferType">Optional: Filter by transfer type (Send, Receive, Both)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of token transfers</returns>
    [HttpGet]
    public async Task<ActionResult<TokenTransferListResponse>> GetTransferHistory(
        [FromQuery] Guid accountId,
        [FromQuery] Guid? tokenId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? transferType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;
            
            var currentUserId = GetCurrentUserId();
            
            // Validate that the user owns the account
            // This would typically involve checking with AccountService
            // For now, we'll proceed with the request
            
            var transfers = await _tokenService.GetTransferHistoryAsync(accountId, tokenId);
            
            // Apply filters
            var filteredTransfers = transfers.AsEnumerable();
            
            if (!string.IsNullOrEmpty(status))
            {
                filteredTransfers = filteredTransfers.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!string.IsNullOrEmpty(transferType))
            {
                switch (transferType.ToLower())
                {
                    case "send":
                        filteredTransfers = filteredTransfers.Where(t => t.FromAccountId == accountId);
                        break;
                    case "receive":
                        filteredTransfers = filteredTransfers.Where(t => t.ToAccountId == accountId);
                        break;
                    case "both":
                    default:
                        // No additional filtering needed
                        break;
                }
            }
            
            // Apply pagination
            var totalCount = filteredTransfers.Count();
            var paginatedTransfers = filteredTransfers
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var response = new TokenTransferListResponse
            {
                Transfers = paginatedTransfers.Select(t => new TokenTransferSummaryResponse
                {
                    TransferId = t.Id,
                    TokenId = t.TokenId,
                    FromAccountId = t.FromAccountId,
                    ToAccountId = t.ToAccountId,
                    Amount = t.Amount,
                    TransactionFee = t.TransactionFee,
                    Status = t.Status,
                    TransferType = t.FromAccountId == accountId ? "Send" : "Receive", // Determine type based on account
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transfer history for account {AccountId}", accountId);
            return StatusCode(500, new { error = "Internal server error occurred while retrieving transfer history" });
        }
    }

    /// <summary>
    /// Get token balance for a specific account and token
    /// </summary>
    /// <param name="accountId">The account ID to get balance for</param>
    /// <param name="tokenId">The token ID to get balance for</param>
    /// <returns>Token balance information</returns>
    [HttpGet("balances/{accountId}/{tokenId}")]
    public async Task<ActionResult<TokenBalanceResponse>> GetTokenBalance(Guid accountId, Guid tokenId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Validate that the user owns the account
            // This would typically involve checking with AccountService
            // For now, we'll proceed with the request
            
            var balance = await _tokenService.GetTokenBalanceAsync(accountId, tokenId);
            
            return Ok(balance);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token balance for account {AccountId} and token {TokenId}", accountId, tokenId);
            return StatusCode(500, new { error = "Internal server error occurred while retrieving token balance" });
        }
    }

    /// <summary>
    /// Get all token balances for a specific account
    /// </summary>
    /// <param name="accountId">The account ID to get all balances for</param>
    /// <returns>List of all token balances for the account</returns>
    [HttpGet("balances/{accountId}")]
    public async Task<ActionResult<List<TokenBalanceResponse>>> GetAllBalances(Guid accountId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Validate that the user owns the account
            // This would typically involve checking with AccountService
            // For now, we'll proceed with the request
            
            var balances = await _tokenService.GetAllBalancesAsync(accountId);
            
            return Ok(balances);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all token balances for account {AccountId}", accountId);
            return StatusCode(500, new { error = "Internal server error occurred while retrieving token balances" });
        }
    }

    /// <summary>
    /// Sync token balance with QuantumLedger
    /// </summary>
    /// <param name="accountId">The account ID to sync balance for</param>
    /// <param name="tokenId">The token ID to sync balance for</param>
    /// <returns>Success confirmation or error message</returns>
    [HttpPost("balances/{accountId}/{tokenId}/sync")]
    public async Task<ActionResult> SyncTokenBalance(Guid accountId, Guid tokenId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Validate that the user owns the account
            // This would typically involve checking with AccountService
            // For now, we'll proceed with the request
            
            var result = await _tokenService.SyncBalanceWithQuantumLedgerAsync(accountId, tokenId);
            
            if (result)
            {
                _logger.LogInformation("Token balance synced with QuantumLedger for account {AccountId} and token {TokenId} by user {UserId}", 
                    accountId, tokenId, currentUserId);
                return Ok(new { message = "Token balance synced with QuantumLedger successfully" });
            }
            
            return BadRequest(new { error = "Failed to sync token balance with QuantumLedger" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing token balance for account {AccountId} and token {TokenId}", accountId, tokenId);
            return StatusCode(500, new { error = "Internal server error occurred while syncing token balance" });
        }
    }

    /// <summary>
    /// Cancel a pending token transfer
    /// </summary>
    /// <param name="transferId">The unique identifier of the token transfer to cancel</param>
    /// <returns>Success confirmation or error message</returns>
    [HttpPut("{transferId}/cancel")]
    public async Task<ActionResult> CancelTransfer(Guid transferId)
    {
        try
        {
            var transfer = await _tokenService.GetTransferAsync(transferId);
            
            if (transfer == null)
            {
                return NotFound(new { error = "Token transfer not found" });
            }
            
            var currentUserId = GetCurrentUserId();
            
            // Validate that the user can cancel this transfer
            // This would typically involve checking account ownership
            // For now, we'll allow cancellation for authenticated users
            
            // Check if transfer can be cancelled
            if (transfer.Status != "Pending")
            {
                return BadRequest(new { error = "Only pending transfers can be cancelled" });
            }
            
            // For now, we'll simulate cancellation by updating the status
            // In a real implementation, this would involve more complex logic
            _logger.LogInformation("Token transfer {TransferId} cancellation requested by user {UserId}", 
                transferId, currentUserId);
            
            return Ok(new { message = "Transfer cancellation requested successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling token transfer {TransferId}", transferId);
            return StatusCode(500, new { error = "Internal server error occurred while cancelling transfer" });
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

// Additional response models for this controller
public class TokenTransferListResponse
{
    public List<TokenTransferSummaryResponse> Transfers { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class TokenTransferSummaryResponse
{
    public Guid TransferId { get; set; }
    public Guid TokenId { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal TransactionFee { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TransferType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class TokenTransferDetailResponse
{
    public Guid TransferId { get; set; }
    public Guid TokenId { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal TransactionFee { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TransferType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? ExternalTransactionHash { get; set; }
    public Guid? QuantumLedgerTransactionId { get; set; }
    public string? MultiChainTransactionId { get; set; }
}
