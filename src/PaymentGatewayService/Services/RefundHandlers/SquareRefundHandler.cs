using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Services.Interfaces;
using PaymentGatewayService.Services.Integrations;
using PaymentGatewayService.Models.Square;
using PaymentGatewayService.Utils;
using PGModels = global::PaymentGatewayService.Models;

namespace PaymentGatewayService.Services;

/// <summary>
/// Square-specific refund handler implementation using the official Square SDK via ISquareService
/// </summary>
public class SquareRefundHandler : IGatewayRefundHandler
{
    private readonly ISquareService _squareService;
    private readonly ILogger<SquareRefundHandler> _logger;

    public SquareRefundHandler(ISquareService squareService, ILogger<SquareRefundHandler> logger)
    {
        _squareService = squareService;
        _logger = logger;
    }

    public async Task<GatewayRefundResult> ProcessRefundAsync(Payment payment, Refund refund)
    {
        try
        {
            _logger.LogInformation("Processing Square refund via SDK. PaymentId: {PaymentId}, RefundId: {RefundId}, Amount: {Amount}",
                payment.Id, refund.Id, refund.Amount);

            if (string.IsNullOrWhiteSpace(payment.GatewayTransactionId))
            {
                _logger.LogError("Square refund failed: Payment {PaymentId} has no GatewayTransactionId", payment.Id);
                return new GatewayRefundResult
                {
                    Status = Data.Entities.RefundStatus.Failed,
                    ErrorMessage = "Original Square payment id (GatewayTransactionId) is missing"
                };
            }

            // Convert refund amount to minor units (cents) using helper
            var currency = string.IsNullOrWhiteSpace(refund.Currency) ? payment.Currency : refund.Currency!;
            var amountMinor = MoneyConverter.ToMinorUnits(refund.Amount, currency);

            var _refundResult = await _squareService.CreateRefundAsync(
                payment.GatewayTransactionId!,
                amountMinor,
                refund.Id.ToString(),
                refund.Reason,
                CancellationToken.None
            );
            var status = _refundResult.Item1;
            var gatewayRefundId = _refundResult.Item2;
            var error = _refundResult.Item3;

            // 'status' returned by ISquareService is the entity PaymentStatus (PaymentGatewayService.Data.Entities.PaymentStatus).
            var mapped = status switch
            {
                Data.Entities.PaymentStatus.Refunded => Data.Entities.RefundStatus.Completed,
                Data.Entities.PaymentStatus.PartiallyRefunded => Data.Entities.RefundStatus.Processing,
                Data.Entities.PaymentStatus.Processing => Data.Entities.RefundStatus.Processing,
                Data.Entities.PaymentStatus.Failed => Data.Entities.RefundStatus.Failed,
                Data.Entities.PaymentStatus.Cancelled => Data.Entities.RefundStatus.Cancelled,
                _ => Data.Entities.RefundStatus.Pending
            };

            if (mapped == Data.Entities.RefundStatus.Failed)
            {
                _logger.LogError("Square refund failed. RefundId: {RefundId}, Error: {Error}", refund.Id, error);
            }
            else
            {
                _logger.LogInformation("Square refund initiated. RefundId: {RefundId}, SquareRefundId: {SquareRefundId}, Status: {Status}",
                    refund.Id, gatewayRefundId, mapped);
            }

            return new GatewayRefundResult
            {
                Status = mapped,
                GatewayRefundId = gatewayRefundId,
                ErrorMessage = error
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Square refund. RefundId: {RefundId}", refund.Id);
            return new GatewayRefundResult
            {
                Status = Data.Entities.RefundStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }
}
