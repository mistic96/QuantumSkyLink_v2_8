using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Services.Interfaces;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EntityRefundStatus = PaymentGatewayService.Data.Entities.RefundStatus;

namespace PaymentGatewayService.Services;

/// <summary>
/// MoonPay-specific refund handler implementation
/// </summary>
public class MoonPayRefundHandler : IGatewayRefundHandler
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MoonPayRefundHandler(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClient = httpClientFactory.CreateClient("MoonPay");
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
            _logger.LogInformation("Processing MoonPay refund. PaymentId: {PaymentId}, RefundId: {RefundId}, Amount: {Amount}",
                payment.Id, refund.Id, refund.Amount);

            // Build MoonPay refund request
            var moonPayRequest = new
            {
                transactionId = payment.GatewayTransactionId,
                amount = refund.Amount,
                currency = refund.Currency.ToUpper(),
                reason = refund.Reason,
                externalRefundId = refund.Id.ToString()
            };

            var json = JsonSerializer.Serialize(moonPayRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add MoonPay API key and signature
            var apiKey = GetMoonPayApiKey();
            var signature = GenerateMoonPaySignature(json);
            
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-SIGNATURE", signature);

            var response = await _httpClient.PostAsync("/v1/refunds", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var moonPayResponse = JsonSerializer.Deserialize<MoonPayRefundResponse>(responseContent, _jsonOptions);
                
                _logger.LogInformation("MoonPay refund initiated successfully. RefundId: {RefundId}, MoonPayRefundId: {MoonPayRefundId}",
                    refund.Id, moonPayResponse?.RefundId);

                return new GatewayRefundResult
                {
                    Status = EntityRefundStatus.Processing,
                    GatewayRefundId = moonPayResponse?.RefundId
                };
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<MoonPayErrorResponse>(responseContent, _jsonOptions);
                var errorMessage = errorResponse?.Message ?? "Unknown MoonPay error";
                
                _logger.LogError("MoonPay refund failed. RefundId: {RefundId}, StatusCode: {StatusCode}, Error: {Error}",
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
            _logger.LogError(ex, "Error processing MoonPay refund. RefundId: {RefundId}", refund.Id);
            return new GatewayRefundResult
            {
                Status = EntityRefundStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    private string GetMoonPayApiKey()
    {
        // In production, this would come from secure configuration
        return Environment.GetEnvironmentVariable("MOONPAY_API_KEY") ?? "test_api_key";
    }

    private string GenerateMoonPaySignature(string payload)
    {
        // In production, this would use the actual secret key
        var secretKey = Environment.GetEnvironmentVariable("MOONPAY_SECRET_KEY") ?? "test_secret_key";
        
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hash);
        }
    }
}

// MoonPay API response models
public class MoonPayRefundResponse
{
    public string? RefundId { get; set; }
    public string? Status { get; set; }
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MoonPayErrorResponse
{
    public string? Type { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
}
