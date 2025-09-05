using Microsoft.EntityFrameworkCore;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Services.Interfaces;
using System.Text.Json;

namespace PaymentGatewayService.Services;

/// <summary>
/// Simplified service implementation for handling refunds and return-to-sender operations
/// </summary>
public class RefundService : IRefundService
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<RefundService> _logger;

    public RefundService(
        PaymentDbContext context,
        ILogger<RefundService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RefundResult> InitiateRefundAsync(RefundRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Initiating refund. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}, Amount: {Amount}", 
            correlationId, request.PaymentId, request.RefundAmount);

        try
        {
            // Get the original payment
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId);

            if (payment == null)
            {
                return new RefundResult
                {
                    Success = false,
                    ErrorMessage = "Payment not found"
                };
            }

            // Validate refund amount
            if (request.RefundAmount > payment.Amount)
            {
                return new RefundResult
                {
                    Success = false,
                    ErrorMessage = "Refund amount exceeds original payment amount"
                };
            }

            // Create refund record
            var refund = new Refund
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                Amount = request.RefundAmount,
                Currency = payment.Currency,
                Status = Data.Entities.RefundStatus.Pending,
                Reason = request.RefundReason,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Refunds.Add(refund);
            await _context.SaveChangesAsync();

            // Simulate gateway processing
            await Task.Delay(100); // Simulate processing time

            // Update status to processing
            refund.Status = Data.Entities.RefundStatus.Processing;
            refund.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refund initiated successfully. RefundId: {RefundId}", refund.Id);

            return new RefundResult
            {
                Success = true,
                RefundId = refund.Id,
                RefundTransactionId = $"refund_{refund.Id:N}",
                Status = new Interfaces.RefundStatus
                {
                    RefundId = refund.Id,
                    Status = refund.Status.ToString(),
                    Amount = refund.Amount,
                    CreatedAt = refund.CreatedAt
                },
                EstimatedCompletionDate = DateTime.UtcNow.AddHours(24)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating refund. CorrelationId: {CorrelationId}", correlationId);
            return new RefundResult
            {
                Success = false,
                ErrorMessage = "An error occurred while processing the refund"
            };
        }
    }

    public async Task<Interfaces.RefundStatus> GetRefundStatusAsync(Guid refundId)
    {
        try
        {
            var refund = await _context.Refunds
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == refundId);

            if (refund == null)
            {
                throw new InvalidOperationException($"Refund with ID {refundId} not found");
            }

            return new Interfaces.RefundStatus
            {
                RefundId = refund.Id,
                Status = refund.Status.ToString(),
                Amount = refund.Amount,
                CreatedAt = refund.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting refund status. RefundId: {RefundId}", refundId);
            throw;
        }
    }

    public async Task<ReturnToSenderResult> ProcessReturnToSenderAsync(Guid paymentId, decimal refundAmount, string reason)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Processing return to sender. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}, Amount: {Amount}", 
            correlationId, paymentId, refundAmount);

        try
        {
            // Get the original payment
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return new ReturnToSenderResult
                {
                    Success = false,
                    ErrorMessage = "Payment not found"
                };
            }

            // Create refund request for return-to-sender
            var refundRequest = new RefundRequest
            {
                PaymentId = paymentId,
                RefundAmount = refundAmount,
                RefundReason = reason,
                RefundType = RefundType.ReturnToSender,
                Metadata = new Dictionary<string, object>
                {
                    ["correlationId"] = correlationId,
                    ["originalAmount"] = payment.Amount,
                    ["feesDeducted"] = payment.Amount - refundAmount
                }
            };

            var refundResult = await InitiateRefundAsync(refundRequest);

            if (refundResult.Success)
            {
                // Update payment status
                payment.Status = PaymentStatus.Refunded;
                payment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Record metrics (monitoring removed)
                _logger.LogInformation("Return to sender succeeded for Payment {PaymentId}, Amount: {Amount}", paymentId, refundAmount);

                return new ReturnToSenderResult
                {
                    Success = true,
                    TransactionId = refundResult.RefundTransactionId,
                    AmountReturned = refundAmount,
                    FeesDeducted = payment.Amount - refundAmount,
                    GatewayType = payment.PaymentGateway.ToString(),
                    ProcessedAt = DateTime.UtcNow
                };
            }
            else
            {
                _logger.LogWarning("Return to sender failed for Payment {PaymentId}. Error: {Error}", paymentId, refundResult.ErrorMessage);

                return new ReturnToSenderResult
                {
                    Success = false,
                    ErrorMessage = refundResult.ErrorMessage,
                    AmountReturned = 0,
                    FeesDeducted = 0,
                    GatewayType = payment.PaymentGateway.ToString(),
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing return to sender. CorrelationId: {CorrelationId}", correlationId);
            return new ReturnToSenderResult
            {
                Success = false,
                ErrorMessage = "An error occurred while processing the return",
                ProcessedAt = DateTime.UtcNow
            };
        }
    }
}
