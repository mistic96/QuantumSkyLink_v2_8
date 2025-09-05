using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EntityRefundStatus = PaymentGatewayService.Data.Entities.RefundStatus;

namespace PaymentGatewayService.Services;

/// <summary>
/// Stripe-specific refund handler implementation
/// </summary>
public class StripeRefundHandler : IGatewayRefundHandler
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public StripeRefundHandler(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClient = httpClientFactory.CreateClient("Stripe");
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<GatewayRefundResult> ProcessRefundAsync(Payment payment, Refund refund)
    {
        try
        {
            _logger.LogInformation("Processing Stripe refund. PaymentId: {PaymentId}, RefundId: {RefundId}, Amount: {Amount}",
                payment.Id, refund.Id, refund.Amount);

            // Build Stripe refund request as form data
            var formData = new Dictionary<string, string>
            {
                ["charge"] = payment.GatewayTransactionId ?? string.Empty,
                ["amount"] = ((long)(refund.Amount * 100)).ToString(), // Convert to cents
                ["currency"] = refund.Currency.ToLower(),
                ["reason"] = MapRefundReason(refund.Reason),
                ["metadata[refund_id]"] = refund.Id.ToString(),
                ["metadata[payment_id]"] = payment.Id.ToString()
            };

            var content = new FormUrlEncodedContent(formData);

            // Add Stripe authorization header
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", GetStripeSecretKey());

            var response = await _httpClient.PostAsync("/v1/refunds", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var stripeResponse = JsonSerializer.Deserialize<StripeRefundResponse>(responseContent, _jsonOptions);
                
                _logger.LogInformation("Stripe refund initiated successfully. RefundId: {RefundId}, StripeRefundId: {StripeRefundId}",
                    refund.Id, stripeResponse?.Id);

                return new GatewayRefundResult
                {
                    Status = EntityRefundStatus.Processing,
                    GatewayRefundId = stripeResponse?.Id
                };
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<StripeErrorResponse>(responseContent, _jsonOptions);
                var errorMessage = errorResponse?.Error?.Message ?? "Unknown Stripe error";
                
                _logger.LogError("Stripe refund failed. RefundId: {RefundId}, StatusCode: {StatusCode}, Error: {Error}",
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
            _logger.LogError(ex, "Error processing Stripe refund. RefundId: {RefundId}", refund.Id);
            return new GatewayRefundResult
            {
                Status = EntityRefundStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    private string GetStripeSecretKey()
    {
        // In production, this would come from secure configuration
        return Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? "sk_test_dummy";
    }

    private string MapRefundReason(string refundType)
    {
        // Map internal refund types to Stripe reasons
        return refundType switch
        {
            "ReturnToSender" => "requested_by_customer",
            "RejectionRefund" => "fraudulent",
            _ => "requested_by_customer"
        };
    }
}

// Stripe API response models
public class StripeRefundResponse
{
    public string? Id { get; set; }
    public string? Object { get; set; }
    public long Amount { get; set; }
    public string? Currency { get; set; }
    public string? Charge { get; set; }
    public string? Status { get; set; }
    public long Created { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class StripeErrorResponse
{
    public StripeError? Error { get; set; }
}

public class StripeError
{
    public string? Type { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Param { get; set; }
}
