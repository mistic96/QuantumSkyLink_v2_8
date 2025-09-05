using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AccountService.Models.Requests;
using AccountService.Models.Responses;
using AccountService.Services.Interfaces;
using AccountService.Data.Entities;
using System.Security.Claims;

namespace AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AccountTransactionController : ControllerBase
{
    private readonly IAccountTransactionService _transactionService;
    private readonly ILogger<AccountTransactionController> _logger;

    public AccountTransactionController(
        IAccountTransactionService transactionService,
        ILogger<AccountTransactionController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new transaction
    /// </summary>
    /// <param name="request">Transaction creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transaction information</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AccountTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountTransactionResponse>> CreateTransaction(
        [FromBody] CreateAccountTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var transaction = await _transactionService.CreateTransactionAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction information</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountTransactionResponse>> GetTransaction(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user owns the transaction
            if (!await _transactionService.UserOwnsTransactionAsync(userId, id, cancellationToken))
            {
                return Forbid("You do not have access to this transaction");
            }

            var transaction = await _transactionService.GetTransactionAsync(id, cancellationToken);
            return Ok(transaction);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction {TransactionId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get transactions for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of transactions</returns>
    [HttpGet("account/{accountId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<AccountTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountTransactionResponse>>> GetAccountTransactions(
        Guid accountId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _transactionService.GetAccountTransactionsAsync(accountId, page, pageSize, cancellationToken);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get transactions for the authenticated user
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user transactions</returns>
    [HttpGet("user")]
    [ProducesResponseType(typeof(IEnumerable<AccountTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountTransactionResponse>>> GetUserTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var transactions = await _transactionService.GetUserTransactionsAsync(userId, page, pageSize, cancellationToken);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get transactions by type for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="transactionType">Transaction type</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of transactions</returns>
    [HttpGet("account/{accountId:guid}/type/{transactionType}")]
    [ProducesResponseType(typeof(IEnumerable<AccountTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountTransactionResponse>>> GetTransactionsByType(
        Guid accountId,
        TransactionType transactionType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsByTypeAsync(accountId, transactionType, page, pageSize, cancellationToken);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {TransactionType} transactions for account {AccountId}", transactionType, accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get transactions by date range for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of transactions</returns>
    [HttpGet("account/{accountId:guid}/range")]
    [ProducesResponseType(typeof(IEnumerable<AccountTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AccountTransactionResponse>>> GetTransactionsByDateRange(
        Guid accountId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest(new { message = "Start date must be before end date" });
            }

            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(accountId, startDate, endDate, page, pageSize, cancellationToken);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for account {AccountId} between {StartDate} and {EndDate}", accountId, startDate, endDate);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update transaction status
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="request">Status update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated transaction</returns>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(AccountTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountTransactionResponse>> UpdateTransactionStatus(
        Guid id,
        [FromBody] UpdateTransactionStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            
            // Verify user owns the transaction
            if (!await _transactionService.UserOwnsTransactionAsync(userId, id, cancellationToken))
            {
                return Forbid("You do not have access to this transaction");
            }

            var transaction = await _transactionService.UpdateTransactionStatusAsync(id, Enum.Parse<TransactionStatus>(request.Status), request.Notes, cancellationToken);
            return Ok(transaction);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction status for {TransactionId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Cancel a transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="request">Cancellation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelTransaction(
        Guid id,
        [FromBody] CancelTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            
            // Verify user owns the transaction
            if (!await _transactionService.UserOwnsTransactionAsync(userId, id, cancellationToken))
            {
                return Forbid("You do not have access to this transaction");
            }

            var result = await _transactionService.CancelTransactionAsync(id, request.Reason, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Transaction not found or cannot be cancelled" });
            }

            return Ok(new { message = "Transaction cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling transaction {TransactionId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get transaction statistics for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="startDate">Start date (optional)</param>
    /// <param name="endDate">End date (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction statistics</returns>
    [HttpGet("account/{accountId:guid}/statistics")]
    [ProducesResponseType(typeof(TransactionStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransactionStatisticsResponse>> GetTransactionStatistics(
        Guid accountId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = await _transactionService.GetTransactionStatisticsAsync(accountId, startDate, endDate, cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction statistics for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Process a deposit transaction
    /// </summary>
    /// <param name="request">Deposit request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processed transaction</returns>
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(AccountTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountTransactionResponse>> ProcessDeposit(
        [FromBody] ProcessDepositRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var transaction = await _transactionService.ProcessDepositAsync(
                request.AccountId, 
                request.Amount, 
                request.Description ?? "Deposit transaction", 
                request.CorrelationId, 
                cancellationToken);

            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing deposit for account {AccountId}", request.AccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Process a withdrawal transaction
    /// </summary>
    /// <param name="request">Withdrawal request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processed transaction</returns>
    [HttpPost("withdrawal")]
    [ProducesResponseType(typeof(AccountTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountTransactionResponse>> ProcessWithdrawal(
        [FromBody] ProcessWithdrawalRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var transaction = await _transactionService.ProcessWithdrawalAsync(
                request.AccountId, 
                request.Amount, 
                request.Description, 
                request.CorrelationId, 
                cancellationToken);

            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing withdrawal for account {AccountId}", request.AccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Process a transfer transaction
    /// </summary>
    /// <param name="request">Transfer request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processed transaction</returns>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(AccountTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountTransactionResponse>> ProcessTransfer(
        [FromBody] ProcessTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var transaction = await _transactionService.ProcessTransferAsync(
                request.FromAccountId, 
                request.ToAccountId, 
                request.Amount, 
                request.Description, 
                request.CorrelationId, 
                cancellationToken);

            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transfer from {FromAccountId} to {ToAccountId}", request.FromAccountId, request.ToAccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // Helper methods
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
}
