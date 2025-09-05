using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Services.Interfaces;
using System.Text;
using System.Text.Json;
using EntityRefundStatus = PaymentGatewayService.Data.Entities.RefundStatus;

namespace PaymentGatewayService.Services;

/// <summary>
/// Dots.dev-specific refund handler implementation for micropayments
/// </summary>
public class DotsDevRefundHandler : IGatewayRefundHandler
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DotsDevRefundHandler(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClient = httpClientFactory.CreateClient("DotsDev");
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<GatewayRefundResult> ProcessRefundAsync(Payment payment, Refund refund)
    {
        try
        {
            _logger.LogInformation("Processing Dots.dev refund. PaymentId: {PaymentId}, RefundId: {RefundId}, Amount: {Amount}",
                payment.Id, refund.Id, refund.Amount);

            // Build Dots.dev refund request
            var dotsRequest = new
            {
                paymentId = payment.GatewayTransactionId,
                amount = new
                {
                    value = (int)(refund.Amount * 100), // Convert to cents
                    currency = refund.Currency.ToLower()
                },
                reason = refund.Reason,
                metadata = new
                {
                    refundId = refund.Id.ToString(),
                    paymentId = payment.Id.ToString(),
                    refundReason = refund.Reason
                }
            };

            var json = JsonSerializer.Serialize(dotsRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Dots.dev uses Basic Auth which is set in HttpClient configuration
            var response = await _httpClient.PostAsync("/payments/refund", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var dotsResponse = JsonSerializer.Deserialize<DotsRefundResponse>(responseContent, _jsonOptions);
                
                _logger.LogInformation("Dots.dev refund initiated successfully. RefundId: {RefundId}, DotsRefundId: {DotsRefundId}",
                    refund.Id, dotsResponse?.RefundId);

                return new GatewayRefundResult
                {
                    Status = EntityRefundStatus.Processing,
                    GatewayRefundId = dotsResponse?.RefundId
                };
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<DotsErrorResponse>(responseContent, _jsonOptions);
                var errorMessage = errorResponse?.Error?.Message ?? "Unknown Dots.dev error";
                
                _logger.LogError("Dots.dev refund failed. RefundId: {RefundId}, StatusCode: {StatusCode}, Error: {Error}",
                    refund.Id, response.StatusCode, errorMessage);

                return new GatewayRefundResult
                {
                    Status = EntityRefundStatus.Failed,
                    ErrorMessage = errorMessage
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Dots.dev refund. RefundId: {RefundId}", refund.Id);
            return new GatewayRefundResult
            {
                Status = EntityRefundStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }
}

// Dots.dev API response models
public class DotsRefundResponse
{
    public string? RefundId { get; set; }
    public string? Status { get; set; }
    public string? PaymentId { get; set; }
    public DotsAmount? Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class DotsAmount
{
    public int Value { get; set; }
    public string? Currency { get; set; }
}

public class DotsErrorResponse
{
    public DotsError? Error { get; set; }
}

public class DotsError
{
    public string? Type { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public Dictionary<string, string>? Details { get; set; }
}
