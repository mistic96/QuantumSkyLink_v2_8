using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TreasuryService.Services.Interfaces;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;

namespace TreasuryService.Controllers;

/// <summary>
/// Controller for treasury transaction processing operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TreasuryTransactionController : ControllerBase
{
    private readonly ITreasuryService _treasuryService;
    private readonly ILogger<TreasuryTransactionController> _logger;

    public TreasuryTransactionController(
        ITreasuryService treasuryService,
        ILogger<TreasuryTransactionController> logger)
    {
        _treasuryService = treasuryService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new treasury transaction
    /// </summary>
    /// <param name="request">Transaction creation request</param>
    /// <returns>Created transaction details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TreasuryTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryTransactionResponse>> CreateTransaction(
        [FromBody] CreateTreasuryTransactionRequest request)
    {
        try
        {
            _logger.LogInformation("Creating treasury transaction: Account {AccountId}, Type {TransactionType}, Amount {Amount} {Currency}", 
                request.AccountId, request.TransactionType, request.Amount, request.Currency);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _treasuryService.CreateTransactionAsync(request);
            
            _logger.LogInformation("Treasury transaction created successfully: {TransactionId}", result.Id);
            return CreatedAtAction(nameof(GetTransaction), new { transactionId = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid treasury transaction creation request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Treasury transaction creation failed: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating treasury transaction: {Request}", request);
            return StatusCode(500, "An error occurred while creating the treasury transaction");
        }
    }

    /// <summary>
    /// Get treasury transaction by ID
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>Transaction details</returns>
    [HttpGet("{transactionId}")]
    [ProducesResponseType(typeof(TreasuryTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryTransactionResponse>> GetTransaction(
        [FromRoute] Guid transactionId)
    {
        try
        {
            _logger.LogInformation("Getting treasury transaction: {TransactionId}", transactionId);

            if (transactionId == Guid.Empty)
            {
                return BadRequest("Transaction ID cannot be empty");
            }

            var transaction = await _treasuryService.GetTransactionAsync(transactionId);
            
            if (transaction == null)
            {
                return NotFound($"Treasury transaction not found: {transactionId}");
            }

            return Ok(transaction);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid treasury transaction request: {TransactionId}", transactionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treasury transaction: {TransactionId}", transactionId);
            return StatusCode(500, "An error occurred while retrieving the treasury transaction");
        }
    }

    /// <summary>
    /// Get treasury transactions with filtering and pagination
    /// </summary>
    /// <param name="accountId">Optional account ID filter</param>
    /// <param name="transactionType">Optional transaction type filter</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="fromDate">Optional start date filter (ISO 8601 format)</param>
    /// <param name="toDate">Optional end date filter (ISO 8601 format)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
    /// <returns>List of treasury transactions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TreasuryTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TreasuryTransactionResponse>>> GetTransactions(
        [FromQuery] Guid? accountId = null,
        [FromQuery] string? transactionType = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Getting treasury transactions - Account: {AccountId}, Type: {TransactionType}, Status: {Status}, FromDate: {FromDate}, ToDate: {ToDate}, Page: {Page}, Size: {PageSize}", 
                accountId, transactionType, status, fromDate, toDate, page, pageSize);

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var request = new GetTreasuryTransactionsRequest
            {
                AccountId = accountId,
                TransactionType = transactionType,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var transactions = await _treasuryService.GetTransactionsAsync(request);
            return Ok(transactions);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid treasury transactions request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treasury transactions");
            return StatusCode(500, "An error occurred while retrieving treasury transactions");
        }
    }

    /// <summary>
    /// Approve a pending treasury transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="request">Approval request with optional notes</param>
    /// <returns>Updated transaction details</returns>
    [HttpPut("{transactionId}/approve")]
    [ProducesResponseType(typeof(TreasuryTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryTransactionResponse>> ApproveTransaction(
        [FromRoute] Guid transactionId,
        [FromBody] ApproveTransactionRequest request)
    {
        try
        {
            _logger.LogInformation("Approving treasury transaction: {TransactionId}", transactionId);

            if (transactionId == Guid.Empty)
            {
                return BadRequest("Transaction ID cannot be empty");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _treasuryService.ApproveTransactionAsync(transactionId, request);
            
            if (result == null)
            {
                return NotFound($"Treasury transaction not found: {transactionId}");
            }

            _logger.LogInformation("Treasury transaction approved successfully: {TransactionId}", transactionId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid treasury transaction approval request: {TransactionId}", transactionId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Treasury transaction approval failed: {TransactionId}", transactionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving treasury transaction: {TransactionId}", transactionId);
            return StatusCode(500, "An error occurred while approving the treasury transaction");
        }
    }

    /// <summary>
    /// Cancel a treasury transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <returns>Updated transaction details</returns>
    [HttpPut("{transactionId}/cancel")]
    [ProducesResponseType(typeof(TreasuryTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryTransactionResponse>> CancelTransaction(
        [FromRoute] Guid transactionId,
        [FromQuery] string reason)
    {
        try
        {
            _logger.LogInformation("Cancelling treasury transaction: {TransactionId}, Reason: {Reason}", transactionId, reason);

            if (transactionId == Guid.Empty)
            {
                return BadRequest("Transaction ID cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest("Cancellation reason is required");
            }

            var result = await _treasuryService.CancelTransactionAsync(transactionId, reason);
            
            if (result == null)
            {
                return NotFound($"Treasury transaction not found: {transactionId}");
            }

            _logger.LogInformation("Treasury transaction cancelled successfully: {TransactionId}", transactionId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid treasury transaction cancellation request: {TransactionId}", transactionId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Treasury transaction cancellation failed: {TransactionId}", transactionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling treasury transaction: {TransactionId}", transactionId);
            return StatusCode(500, "An error occurred while cancelling the treasury transaction");
        }
    }

    /// <summary>
    /// Transfer funds between treasury accounts
    /// </summary>
    /// <param name="request">Fund transfer request</param>
    /// <returns>Transfer transaction details</returns>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TreasuryTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryTransactionResponse>> TransferFunds(
        [FromBody] TransferFundsRequest request)
    {
        try
        {
            _logger.LogInformation("Transferring funds: From {FromAccountId} to {ToAccountId}, Amount {Amount} {Currency}", 
                request.FromAccountId, request.ToAccountId, request.Amount, request.Currency);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.FromAccountId == request.ToAccountId)
            {
                return BadRequest("From account and to account cannot be the same");
            }

            var result = await _treasuryService.TransferFundsAsync(request);
            
            _logger.LogInformation("Fund transfer completed successfully: {TransactionId}", result.Id);
            return CreatedAtAction(nameof(GetTransaction), new { transactionId = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fund transfer request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Fund transfer failed: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring funds: {Request}", request);
            return StatusCode(500, "An error occurred while transferring funds");
        }
    }

    /// <summary>
    /// Deposit funds into a treasury account
    /// </summary>
    /// <param name="request">Fund deposit request</param>
    /// <returns>Deposit transaction details</returns>
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(TreasuryTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryTransactionResponse>> DepositFunds(
        [FromBody] DepositFundsRequest request)
    {
        try
        {
            _logger.LogInformation("Depositing funds: Account {AccountId}, Amount {Amount} {Currency}, Method {PaymentMethod}", 
                request.AccountId, request.Amount, request.Currency, request.PaymentMethod);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _treasuryService.DepositFundsAsync(request);
            
            _logger.LogInformation("Fund deposit completed successfully: {TransactionId}", result.Id);
            return CreatedAtAction(nameof(GetTransaction), new { transactionId = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fund deposit request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Fund deposit failed: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error depositing funds: {Request}", request);
            return StatusCode(500, "An error occurred while depositing funds");
        }
    }

    /// <summary>
    /// Withdraw funds from a treasury account
    /// </summary>
    /// <param name="request">Fund withdrawal request</param>
    /// <returns>Withdrawal transaction details</returns>
    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(TreasuryTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryTransactionResponse>> WithdrawFunds(
        [FromBody] WithdrawFundsRequest request)
    {
        try
        {
            _logger.LogInformation("Withdrawing funds: Account {AccountId}, Amount {Amount} {Currency}, Method {PaymentMethod}", 
                request.AccountId, request.Amount, request.Currency, request.PaymentMethod);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _treasuryService.WithdrawFundsAsync(request);
            
            _logger.LogInformation("Fund withdrawal completed successfully: {TransactionId}", result.Id);
            return CreatedAtAction(nameof(GetTransaction), new { transactionId = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fund withdrawal request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Fund withdrawal failed: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing funds: {Request}", request);
            return StatusCode(500, "An error occurred while withdrawing funds");
        }
    }
}
