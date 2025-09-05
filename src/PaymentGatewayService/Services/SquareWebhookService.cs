using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.Square;
using PaymentGatewayService.Services.Integrations;

namespace PaymentGatewayService.Services;

/// <summary>
/// Encapsulates Square webhook verification and event-to-status mapping logic.
/// Delegates signature verification to ISquareService and centralizes mapping rules.
/// </summary>
public class SquareWebhookService
{
    private readonly ISquareService _squareService;
    private readonly ILogger<SquareWebhookService> _logger;

    public SquareWebhookService(ISquareService squareService, ILogger<SquareWebhookService> logger)
    {
        _squareService = squareService;
        _logger = logger;
    }

    /// <summary>
    /// Verify webhook signature using configured secret. Returns (Valid, Error).
    /// </summary>
    public Task<(bool Valid, string? Error)> VerifyAsync(string signature, string requestBody, string requestUrl, CancellationToken ct = default)
        => _squareService.VerifyWebhookSignatureAsync(signature, requestBody, requestUrl, ct);

    /// <summary>
    /// Map a Square event (type + optional payment status) to an internal PaymentStatus.
    /// Prefer direct payment status mapping when available.
    /// </summary>
    public Task<Data.Entities.PaymentStatus> MapEventToStatusAsync(string? eventType, string? squarePaymentStatus)
    {
        // If Square provided the payment status string, use our status mapper first
        if (!string.IsNullOrWhiteSpace(squarePaymentStatus))
        {
            var mapped = SquareStatusMapper.MapPaymentStatus(squarePaymentStatus);
            return Task.FromResult(mapped);
        }

        // Fallback: infer from event type
        var mappedFromEvent = (eventType ?? string.Empty).ToLowerInvariant() switch
        {
            "payment.created" => Data.Entities.PaymentStatus.Pending,
            "payment.approved" => Data.Entities.PaymentStatus.Processing,
            "payment.captured" => Data.Entities.PaymentStatus.Completed,
            "payment.completed" => Data.Entities.PaymentStatus.Completed,
            "payment.updated" => Data.Entities.PaymentStatus.Processing,
            "payment.canceled" => Data.Entities.PaymentStatus.Cancelled,
            "payment.cancelled" => Data.Entities.PaymentStatus.Cancelled,
            "payment.failed" => Data.Entities.PaymentStatus.Failed,
            _ => Data.Entities.PaymentStatus.Processing
        };

        return Task.FromResult(mappedFromEvent);
    }
}
