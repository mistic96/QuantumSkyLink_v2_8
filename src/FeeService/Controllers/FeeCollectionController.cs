using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FeeService.Services.Interfaces;
using FeeService.Models.Requests;
using FeeService.Models.Responses;

namespace FeeService.Controllers;

/// <summary>
/// Controller for fee collection and payment processing operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class FeeCollectionController : ControllerBase
{
    private readonly IFeeCollectionService _feeCollectionService;
    private readonly ILogger<FeeCollectionController> _logger;

    public FeeCollectionController(
        IFeeCollectionService feeCollectionService,
        ILogger<FeeCollectionController> logger)
    {
        _feeCollectionService = feeCollectionService;
        _logger = logger;
    }

    /// <summary>
    /// Collect fee from a user
    /// </summary>
    /// <param name="request">Fee collection request</param>
    /// <returns>Fee transaction result</returns>
    [HttpPost("collect")]
    [ProducesResponseType(typeof(FeeTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FeeTransactionResponse>> CollectFee(
        [FromBody] CollectFeeRequest request)
    {
        try
        {
            _logger.LogInformation("Collecting fee for user {UserId}, calculation result {CalculationResultId}", 
                request.UserId, request.CalculationResultId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _feeCollectionService.CollectFeeAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fee collection request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting fee: {Request}", request);
            return StatusCode(500, "An error occurred while collecting the fee");
        }
    }

    /// <summary>
    /// Process refund for a fee transaction
    /// </summary>
    /// <param name="request">Refund processing request</param>
    /// <returns>Refund transaction result</returns>
    [HttpPost("refund")]
    [ProducesResponseType(typeof(FeeTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FeeTransactionResponse>> ProcessRefund(
        [FromBody] ProcessRefundRequest request)
    {
        try
        {
            _logger.LogInformation("Processing refund for transaction {TransactionId}, amount {Amount}", 
                request.TransactionId, request.RefundAmount);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _feeCollectionService.ProcessRefundAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid refund request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot process refund for transaction {TransactionId}", request.TransactionId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund: {Request}", request);
            return StatusCode(500, "An error occurred while processing the refund");
        }
    }

    /// <summary>
    /// Get fee transaction details
    /// </summary>
    /// <param name="transactionId">Transaction ID to retrieve</param>
    /// <returns>Transaction details</returns>
    [HttpGet("transactions/{transactionId}")]
    [ProducesResponseType(typeof(FeeTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FeeTransactionResponse>> GetTransaction(
        [FromRoute] Guid transactionId)
    {
        try
        {
            _logger.LogInformation("Getting transaction details for {TransactionId}", transactionId);

            if (transactionId == Guid.Empty)
            {
                return BadRequest("Transaction ID cannot be empty");
            }

            var transaction = await _feeCollectionService.GetTransactionAsync(transactionId);
            
            if (transaction == null)
            {
                return NotFound($"Transaction {transactionId} not found");
            }

            return Ok(transaction);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid transaction request for {TransactionId}", transactionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction {TransactionId}", transactionId);
            return StatusCode(500, "An error occurred while retrieving the transaction");
        }
    }

    /// <summary>
    /// Get fee transaction history for a user
    /// </summary>
    /// <param name="userId">User ID to get history for</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
    /// <param name="status">Filter by transaction status (optional)</param>
    /// <param name="fromDate">Start date for filtering (optional)</param>
    /// <param name="toDate">End date for filtering (optional)</param>
    /// <returns>List of user's fee transactions</returns>
    [HttpGet("transactions/history/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<FeeTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FeeTransactionResponse>>> GetTransactionHistory(
        [FromRoute] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation("Getting transaction history for user {UserId}, page {Page}, size {PageSize}", 
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

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest("From date cannot be after to date");
            }

            var history = await _feeCollectionService.GetTransactionHistoryAsync(
                userId, page, pageSize, status, fromDate, toDate);
            
            return Ok(history);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid transaction history request for user {UserId}", userId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction history for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving transaction history");
        }
    }

    /// <summary>
    /// Update transaction status
    /// </summary>
    /// <param name="transactionId">Transaction ID to update</param>
    /// <param name="status">New status for the transaction</param>
    /// <param name="reason">Optional reason for the status change</param>
    /// <returns>Updated transaction details</returns>
    [HttpPut("transactions/{transactionId}/status")]
    [ProducesResponseType(typeof(FeeTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FeeTransactionResponse>> UpdateTransactionStatus(
        [FromRoute] Guid transactionId,
        [FromQuery] string status,
        [FromQuery] string? reason = null)
    {
        try
        {
            _logger.LogInformation("Updating transaction {TransactionId} status to {Status}", 
                transactionId, status);

            if (transactionId == Guid.Empty)
            {
                return BadRequest("Transaction ID cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest("Status cannot be empty");
            }

            var result = await _feeCollectionService.UpdateTransactionStatusAsync(transactionId, status, reason);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid status update request for transaction {TransactionId}", transactionId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot update status for transaction {TransactionId}", transactionId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction status for {TransactionId}", transactionId);
            return StatusCode(500, "An error occurred while updating the transaction status");
        }
    }

    /// <summary>
    /// Generate receipt for a transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID to generate receipt for</param>
    /// <returns>Receipt details</returns>
    [HttpGet("transactions/{transactionId}/receipt")]
    [ProducesResponseType(typeof(ReceiptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReceiptResponse>> GenerateReceipt(
        [FromRoute] Guid transactionId)
    {
        try
        {
            _logger.LogInformation("Generating receipt for transaction {TransactionId}", transactionId);

            if (transactionId == Guid.Empty)
            {
                return BadRequest("Transaction ID cannot be empty");
            }

            var receipt = await _feeCollectionService.GenerateReceiptAsync(transactionId);
            
            if (receipt == null)
            {
                return NotFound($"Cannot generate receipt for transaction {transactionId}");
            }

            return Ok(receipt);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid receipt generation request for transaction {TransactionId}", transactionId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot generate receipt for transaction {TransactionId}", transactionId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating receipt for transaction {TransactionId}", transactionId);
            return StatusCode(500, "An error occurred while generating the receipt");
        }
    }

    /// <summary>
    /// Validate payment method for a user
    /// </summary>
    /// <param name="paymentMethod">Payment method to validate</param>
    /// <param name="userId">User ID to validate payment method for</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate-payment-method")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> ValidatePaymentMethod(
        [FromQuery] string paymentMethod,
        [FromQuery] Guid userId)
    {
        try
        {
            _logger.LogInformation("Validating payment method {PaymentMethod} for user {UserId}", 
                paymentMethod, userId);

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                return BadRequest("Payment method cannot be empty");
            }

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty");
            }

            var isValid = await _feeCollectionService.ValidatePaymentMethodAsync(paymentMethod, userId);
            return Ok(isValid);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid payment method validation request: {PaymentMethod}, {UserId}", 
                paymentMethod, userId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment method {PaymentMethod} for user {UserId}", 
                paymentMethod, userId);
            return StatusCode(500, "An error occurred while validating the payment method");
        }
    }

    /// <summary>
    /// Get pending transactions (admin endpoint)
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
    /// <returns>List of pending transactions</returns>
    [HttpGet("transactions/pending")]
    [ProducesResponseType(typeof(IEnumerable<FeeTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FeeTransactionResponse>>> GetPendingTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Getting pending transactions, page {Page}, size {PageSize}", 
                page, pageSize);

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var pendingTransactions = await _feeCollectionService.GetPendingTransactionsAsync(page, pageSize);
            return Ok(pendingTransactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending transactions");
            return StatusCode(500, "An error occurred while retrieving pending transactions");
        }
    }
}
