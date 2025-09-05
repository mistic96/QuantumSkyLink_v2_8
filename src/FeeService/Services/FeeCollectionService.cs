using Microsoft.EntityFrameworkCore;
using FeeService.Data;
using FeeService.Data.Entities;
using FeeService.Models.Requests;
using FeeService.Models.Responses;
using FeeService.Services.Interfaces;
using Mapster;

namespace FeeService.Services;

public class FeeCollectionService : IFeeCollectionService
{
    private readonly FeeDbContext _context;
    private readonly ILogger<FeeCollectionService> _logger;
    private readonly IConfiguration _configuration;

    private readonly bool _mockMode;

    public FeeCollectionService(
        FeeDbContext context,
        ILogger<FeeCollectionService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _mockMode = _configuration.GetValue<bool>("FeeService:EnableMockMode", true);
    }

    public async Task<FeeTransactionResponse> CollectFeeAsync(CollectFeeRequest request)
    {
        try
        {
            _logger.LogInformation("Collecting fee for user {UserId}, calculation result {CalculationResultId}", 
                request.UserId, request.CalculationResultId);

            // Get the calculation result
            var calculationResult = await _context.FeeCalculationResults
                .Include(cr => cr.FeeConfiguration)
                .FirstOrDefaultAsync(cr => cr.Id == request.CalculationResultId && cr.UserId == request.UserId);

            if (calculationResult == null)
            {
                throw new InvalidOperationException($"Calculation result not found or does not belong to user: {request.CalculationResultId}");
            }

            // Validate payment method
            if (!await ValidatePaymentMethodAsync(request.PaymentMethod, request.UserId))
            {
                throw new ArgumentException($"Invalid payment method: {request.PaymentMethod}");
            }

            // Create fee transaction
            var feeTransaction = new FeeTransaction
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                TransactionType = "Collection",
                ReferenceId = calculationResult.ReferenceId,
                ReferenceType = calculationResult.ReferenceType,
                Amount = calculationResult.FinalFeeAmount,
                Currency = calculationResult.FeeCurrency,
                Status = "Pending",
                Description = request.Description ?? $"Fee collection for {calculationResult.FeeType}",
                PaymentMethod = request.PaymentMethod,
                PaymentReference = request.PaymentReference,
                FeeConfigurationId = calculationResult.FeeConfigurationId,
                CalculationResultId = calculationResult.Id,
                Metadata = request.Metadata != null ? System.Text.Json.JsonSerializer.Serialize(request.Metadata) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FeeTransactions.Add(feeTransaction);
            await _context.SaveChangesAsync();

            // Process payment (mock or real)
            var paymentResult = await ProcessPaymentAsync(feeTransaction);

            // Update transaction status
            feeTransaction.Status = paymentResult.Success ? "Completed" : "Failed";
            feeTransaction.ProcessedAt = DateTime.UtcNow;
            feeTransaction.FailureReason = paymentResult.FailureReason;
            feeTransaction.PaymentReference = paymentResult.PaymentReference ?? feeTransaction.PaymentReference;
            feeTransaction.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = feeTransaction.Adapt<FeeTransactionResponse>();
            response.FeeConfiguration = calculationResult.FeeConfiguration?.Adapt<FeeConfigurationResponse>();
            response.CalculationResult = calculationResult.Adapt<FeeCalculationResponse>();

            _logger.LogInformation("Fee collection {Status} for user {UserId}, transaction {TransactionId}", 
                feeTransaction.Status, request.UserId, feeTransaction.Id);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting fee for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<FeeTransactionResponse> ProcessRefundAsync(ProcessRefundRequest request)
    {
        try
        {
            _logger.LogInformation("Processing refund for transaction {TransactionId}, amount {Amount}", 
                request.TransactionId, request.RefundAmount);

            // Get the original transaction
            var originalTransaction = await _context.FeeTransactions
                .Include(ft => ft.FeeConfiguration)
                .Include(ft => ft.CalculationResult)
                .FirstOrDefaultAsync(ft => ft.Id == request.TransactionId);

            if (originalTransaction == null)
            {
                throw new InvalidOperationException($"Transaction not found: {request.TransactionId}");
            }

            if (originalTransaction.Status != "Completed")
            {
                throw new InvalidOperationException($"Cannot refund transaction with status: {originalTransaction.Status}");
            }

            if (request.RefundAmount > originalTransaction.Amount)
            {
                throw new ArgumentException("Refund amount cannot exceed original transaction amount");
            }

            // Check if already refunded
            var existingRefund = await _context.FeeTransactions
                .Where(ft => ft.ReferenceId == originalTransaction.Id.ToString() && 
                            ft.TransactionType == "Refund" && 
                            ft.Status == "Completed")
                .SumAsync(ft => ft.Amount);

            if (existingRefund + request.RefundAmount > originalTransaction.Amount)
            {
                throw new InvalidOperationException("Total refund amount would exceed original transaction amount");
            }

            // Create refund transaction
            var refundTransaction = new FeeTransaction
            {
                Id = Guid.NewGuid(),
                UserId = originalTransaction.UserId,
                TransactionType = "Refund",
                ReferenceId = originalTransaction.Id.ToString(),
                ReferenceType = "FeeTransaction",
                Amount = request.RefundAmount,
                Currency = originalTransaction.Currency,
                Status = "Pending",
                Description = $"Refund for transaction {originalTransaction.Id}: {request.Reason}",
                PaymentMethod = originalTransaction.PaymentMethod,
                FeeConfigurationId = originalTransaction.FeeConfigurationId,
                CalculationResultId = originalTransaction.CalculationResultId,
                Metadata = request.Metadata != null ? System.Text.Json.JsonSerializer.Serialize(request.Metadata) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FeeTransactions.Add(refundTransaction);
            await _context.SaveChangesAsync();

            // Process refund (mock or real)
            var refundResult = await ProcessRefundPaymentAsync(refundTransaction, originalTransaction);

            // Update refund transaction status
            refundTransaction.Status = refundResult.Success ? "Completed" : "Failed";
            refundTransaction.ProcessedAt = DateTime.UtcNow;
            refundTransaction.FailureReason = refundResult.FailureReason;
            refundTransaction.PaymentReference = refundResult.PaymentReference;
            refundTransaction.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = refundTransaction.Adapt<FeeTransactionResponse>();
            response.FeeConfiguration = originalTransaction.FeeConfiguration?.Adapt<FeeConfigurationResponse>();
            response.CalculationResult = originalTransaction.CalculationResult?.Adapt<FeeCalculationResponse>();

            _logger.LogInformation("Refund {Status} for transaction {TransactionId}, refund transaction {RefundTransactionId}", 
                refundTransaction.Status, request.TransactionId, refundTransaction.Id);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for transaction {TransactionId}", request.TransactionId);
            throw;
        }
    }

    public async Task<FeeTransactionResponse> GetTransactionAsync(Guid transactionId)
    {
        try
        {
            var transaction = await _context.FeeTransactions
                .Include(ft => ft.FeeConfiguration)
                .Include(ft => ft.CalculationResult)
                .Include(ft => ft.Distributions)
                .FirstOrDefaultAsync(ft => ft.Id == transactionId);

            if (transaction == null)
            {
                throw new InvalidOperationException($"Transaction not found: {transactionId}");
            }

            var response = transaction.Adapt<FeeTransactionResponse>();
            response.FeeConfiguration = transaction.FeeConfiguration?.Adapt<FeeConfigurationResponse>();
            response.CalculationResult = transaction.CalculationResult?.Adapt<FeeCalculationResponse>();
            response.Distributions = transaction.Distributions?.Adapt<IEnumerable<FeeDistributionResponse>>();

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<IEnumerable<FeeTransactionResponse>> GetTransactionHistoryAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            
            var query = _context.FeeTransactions
                .Where(ft => ft.UserId == userId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(ft => ft.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(ft => ft.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(ft => ft.CreatedAt <= toDate.Value);
            }

            var transactions = await query
                .OrderByDescending(ft => ft.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Include(ft => ft.FeeConfiguration)
                .Include(ft => ft.CalculationResult)
                .ToListAsync();

            return transactions.Adapt<IEnumerable<FeeTransactionResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction history for user {UserId}", userId);
            throw;
        }
    }

    public async Task<FeeTransactionResponse> UpdateTransactionStatusAsync(Guid transactionId, string status, string? reason = null)
    {
        try
        {
            var transaction = await _context.FeeTransactions
                .Include(ft => ft.FeeConfiguration)
                .Include(ft => ft.CalculationResult)
                .FirstOrDefaultAsync(ft => ft.Id == transactionId);

            if (transaction == null)
            {
                throw new InvalidOperationException($"Transaction not found: {transactionId}");
            }

            var oldStatus = transaction.Status;
            transaction.Status = status;
            transaction.UpdatedAt = DateTime.UtcNow;

            if (status == "Failed" && !string.IsNullOrEmpty(reason))
            {
                transaction.FailureReason = reason;
            }

            if (status == "Completed" && oldStatus == "Pending")
            {
                transaction.ProcessedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var response = transaction.Adapt<FeeTransactionResponse>();
            response.FeeConfiguration = transaction.FeeConfiguration?.Adapt<FeeConfigurationResponse>();
            response.CalculationResult = transaction.CalculationResult?.Adapt<FeeCalculationResponse>();

            _logger.LogInformation("Transaction {TransactionId} status updated from {OldStatus} to {NewStatus}", 
                transactionId, oldStatus, status);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction status for {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<ReceiptResponse> GenerateReceiptAsync(Guid transactionId)
    {
        try
        {
            var transaction = await _context.FeeTransactions
                .Include(ft => ft.FeeConfiguration)
                .Include(ft => ft.CalculationResult)
                .FirstOrDefaultAsync(ft => ft.Id == transactionId);

            if (transaction == null)
            {
                throw new InvalidOperationException($"Transaction not found: {transactionId}");
            }

            var receiptNumber = GenerateReceiptNumber(transaction);

            var receipt = new ReceiptResponse
            {
                TransactionId = transaction.Id,
                ReceiptNumber = receiptNumber,
                UserId = transaction.UserId,
                TransactionType = transaction.TransactionType,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Status = transaction.Status,
                TransactionDate = transaction.CreatedAt,
                Description = transaction.Description,
                PaymentMethod = transaction.PaymentMethod,
                PaymentReference = transaction.PaymentReference,
                GeneratedAt = DateTime.UtcNow,
                TransactionDetails = new
                {
                    ReferenceId = transaction.ReferenceId,
                    ReferenceType = transaction.ReferenceType,
                    ProcessedAt = transaction.ProcessedAt,
                    ConvertedAmount = transaction.ConvertedAmount,
                    ConvertedCurrency = transaction.ConvertedCurrency,
                    ExchangeRate = transaction.ExchangeRate
                },
                FeeBreakdown = transaction.CalculationResult != null ? new
                {
                    BaseAmount = transaction.CalculationResult.BaseAmount,
                    BaseCurrency = transaction.CalculationResult.BaseCurrency,
                    CalculatedFee = transaction.CalculationResult.CalculatedFee,
                    DiscountAmount = transaction.CalculationResult.DiscountAmount,
                    DiscountPercentage = transaction.CalculationResult.DiscountPercentage,
                    FinalFeeAmount = transaction.CalculationResult.FinalFeeAmount
                } : null
            };

            _logger.LogInformation("Generated receipt {ReceiptNumber} for transaction {TransactionId}", 
                receiptNumber, transactionId);

            return receipt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating receipt for transaction {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<bool> ValidatePaymentMethodAsync(string paymentMethod, Guid userId)
    {
        try
        {
            // In mock mode, accept common payment methods
            if (_mockMode)
            {
                var validMockMethods = new[] { "CreditCard", "DebitCard", "BankTransfer", "Crypto", "Wallet", "PayPal" };
                return validMockMethods.Contains(paymentMethod, StringComparer.OrdinalIgnoreCase);
            }

            // In real mode, you would validate against actual payment provider
            // This could involve checking user's registered payment methods, etc.
            
            _logger.LogDebug("Validating payment method {PaymentMethod} for user {UserId}", paymentMethod, userId);
            
            // Placeholder for real validation logic
            return !string.IsNullOrWhiteSpace(paymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment method {PaymentMethod} for user {UserId}", paymentMethod, userId);
            return false;
        }
    }

    public async Task<IEnumerable<FeeTransactionResponse>> GetPendingTransactionsAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            
            var transactions = await _context.FeeTransactions
                .Where(ft => ft.Status == "Pending")
                .OrderBy(ft => ft.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Include(ft => ft.FeeConfiguration)
                .Include(ft => ft.CalculationResult)
                .ToListAsync();

            return transactions.Adapt<IEnumerable<FeeTransactionResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending transactions");
            throw;
        }
    }

    private async Task<PaymentResult> ProcessPaymentAsync(FeeTransaction transaction)
    {
        try
        {
            if (_mockMode)
            {
                // Mock payment processing
                await Task.Delay(100); // Simulate processing time
                
                // 95% success rate in mock mode
                var success = Random.Shared.NextDouble() > 0.05;
                
                return new PaymentResult
                {
                    Success = success,
                    PaymentReference = success ? $"MOCK_PAY_{Guid.NewGuid():N}[..8]" : null,
                    FailureReason = success ? null : "Mock payment failure for testing"
                };
            }

            // Real payment processing would go here
            // This would integrate with actual payment providers
            
            _logger.LogInformation("Processing real payment for transaction {TransactionId}", transaction.Id);
            
            // Placeholder for real payment processing
            return new PaymentResult
            {
                Success = true,
                PaymentReference = $"REAL_PAY_{Guid.NewGuid():N}[..8]"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for transaction {TransactionId}", transaction.Id);
            return new PaymentResult
            {
                Success = false,
                FailureReason = ex.Message
            };
        }
    }

    private async Task<PaymentResult> ProcessRefundPaymentAsync(FeeTransaction refundTransaction, FeeTransaction originalTransaction)
    {
        try
        {
            if (_mockMode)
            {
                // Mock refund processing
                await Task.Delay(100); // Simulate processing time
                
                // 98% success rate for refunds in mock mode
                var success = Random.Shared.NextDouble() > 0.02;
                
                return new PaymentResult
                {
                    Success = success,
                    PaymentReference = success ? $"MOCK_REF_{Guid.NewGuid():N}[..8]" : null,
                    FailureReason = success ? null : "Mock refund failure for testing"
                };
            }

            // Real refund processing would go here
            _logger.LogInformation("Processing real refund for transaction {TransactionId}", refundTransaction.Id);
            
            // Placeholder for real refund processing
            return new PaymentResult
            {
                Success = true,
                PaymentReference = $"REAL_REF_{Guid.NewGuid():N}[..8]"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for transaction {TransactionId}", refundTransaction.Id);
            return new PaymentResult
            {
                Success = false,
                FailureReason = ex.Message
            };
        }
    }

    private string GenerateReceiptNumber(FeeTransaction transaction)
    {
        var date = transaction.CreatedAt.ToString("yyyyMMdd");
        var transactionShort = transaction.Id.ToString("N")[..8].ToUpper();
        return $"RCP-{date}-{transactionShort}";
    }

    private class PaymentResult
    {
        public bool Success { get; set; }
        public string? PaymentReference { get; set; }
        public string? FailureReason { get; set; }
    }
}
