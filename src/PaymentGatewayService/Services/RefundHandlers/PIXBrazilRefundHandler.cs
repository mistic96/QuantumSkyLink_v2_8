using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Services.Interfaces;
using System.Text;
using System.Text.Json;
using EntityRefundStatus = PaymentGatewayService.Data.Entities.RefundStatus;

namespace PaymentGatewayService.Services;

/// <summary>
/// PIX Brazil-specific refund handler implementation
/// </summary>
public class PIXBrazilRefundHandler : IGatewayRefundHandler
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PIXBrazilRefundHandler(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClient = httpClientFactory.CreateClient("PIXBrazil");
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
            _logger.LogInformation("Processing PIX Brazil refund. PaymentId: {PaymentId}, RefundId: {RefundId}, Amount: {Amount}",
                payment.Id, refund.Id, refund.Amount);

            // Extract PIX transaction details from payment metadata
            var pixTransactionId = ExtractPixTransactionId(payment.Metadata);
            var pixKey = ExtractPixKey(payment.Metadata);

            // Build PIX refund request
            var pixRequest = new
            {
                idempotencyKey = refund.Id.ToString(),
                originalTransactionId = pixTransactionId,
                amount = refund.Amount,
                currency = "BRL", // PIX only supports Brazilian Real
                description = refund.Reason,
                refundInfo = new
                {
                    pixKey = pixKey,
                    refundType = "TOTAL", // Can be TOTAL or PARTIAL
                    refundReason = MapRefundReason(refund.Reason)
                }
            };

            var json = JsonSerializer.Serialize(pixRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/pix/refunds", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var pixResponse = JsonSerializer.Deserialize<PIXRefundResponse>(responseContent, _jsonOptions);
                
                _logger.LogInformation("PIX refund initiated successfully. RefundId: {RefundId}, PIXRefundId: {PIXRefundId}",
                    refund.Id, pixResponse?.RefundId);

                return new GatewayRefundResult
                {
                    Status = EntityRefundStatus.Processing,
                    GatewayRefundId = pixResponse?.RefundId
                };
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<PIXErrorResponse>(responseContent, _jsonOptions);
                var errorMessage = errorResponse?.Error?.Message ?? "Unknown PIX error";
                
                _logger.LogError("PIX refund failed. RefundId: {RefundId}, StatusCode: {StatusCode}, Error: {Error}",
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
            _logger.LogError(ex, "Error processing PIX refund. RefundId: {RefundId}", refund.Id);
            return new GatewayRefundResult
            {
                Status = EntityRefundStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    private string ExtractPixTransactionId(string? metadata)
    {
        if (string.IsNullOrEmpty(metadata))
            return string.Empty;

        try
        {
            var metadataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(metadata);
            if (metadataDict != null && metadataDict.TryGetValue("pix_transaction_id", out var id))
            {
                return id.ToString() ?? string.Empty;
            }
        }
        catch
        {
            // Ignore deserialization errors
        }

        return string.Empty;
    }

    private string ExtractPixKey(string? metadata)
    {
        if (string.IsNullOrEmpty(metadata))
            return string.Empty;

        try
        {
            var metadataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(metadata);
            if (metadataDict != null && metadataDict.TryGetValue("pix_key", out var key))
            {
                return key.ToString() ?? string.Empty;
            }
        }
        catch
        {
            // Ignore deserialization errors
        }

        return string.Empty;
    }

    private string MapRefundReason(string refundType)
    {
        // Map internal refund types to PIX refund reasons
        return refundType switch
        {
            "ReturnToSender" => "CUSTOMER_REQUEST",
            "RejectionRefund" => "FRAUD",
            _ => "OTHER"
        };
    }
}

// PIX API response models
public class PIXRefundResponse
{
    public string? RefundId { get; set; }
    public string? Status { get; set; }
    public string? OriginalTransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class PIXErrorResponse
{
    public PIXError? Error { get; set; }
}

public class PIXError
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Field { get; set; }
}
