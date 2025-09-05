using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGatewayService.Configuration;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.Square;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PaymentGatewayService.Services.Integrations;

public class SquareService : ISquareService
{
    private readonly SquareConfiguration _config;
    private readonly ILogger<SquareService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public SquareService(IOptions<SquareConfiguration> config, ILogger<SquareService> logger, IHttpClientFactory httpClientFactory)
    {
        _config = config.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<(PaymentStatus Status, string? GatewayTransactionId, string? Error)> CreatePaymentAsync(
        SquarePaymentRequest request,
        CancellationToken ct)
    {
        try
        {
            // TODO: Implement Square payment creation using HTTP client
            // This is a placeholder implementation
            _logger.LogWarning("Square CreatePaymentAsync is not fully implemented");
            
            // For now, return a mock successful response
            var gatewayId = Guid.NewGuid().ToString();
            return (PaymentStatus.Processing, gatewayId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Square CreatePaymentAsync unexpected error");
            return (PaymentStatus.Failed, null, ex.Message);
        }
    }

    public async Task<(PaymentStatus Status, string? Error)> GetPaymentStatusAsync(string gatewayPaymentId, CancellationToken ct)
    {
        try
        {
            // TODO: Implement Square payment status retrieval
            _logger.LogWarning("Square GetPaymentStatusAsync is not fully implemented");
            return (PaymentStatus.Completed, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Square GetPaymentStatusAsync unexpected error");
            return (PaymentStatus.Failed, ex.Message);
        }
    }

    public async Task<(PaymentStatus Status, string? GatewayRefundId, string? Error)> CreateRefundAsync(
        string paymentId,
        long amountMinor,
        string idempotencyKey,
        string? reason,
        CancellationToken ct)
    {
        try
        {
            // TODO: Implement Square refund creation
            _logger.LogWarning("Square CreateRefundAsync is not fully implemented");
            var refundId = Guid.NewGuid().ToString();
            return (PaymentStatus.Refunded, refundId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Square CreateRefundAsync unexpected error");
            return (PaymentStatus.Failed, null, ex.Message);
        }
    }

    public Task<(bool Valid, string? Error)> VerifyWebhookSignatureAsync(
        string signatureHeader,
        string requestBody,
        string requestUrl,
        CancellationToken ct)
    {
        try
        {
            // If no secret configured, consider verification disabled in dev
            if (string.IsNullOrWhiteSpace(_config.WebhookSignatureKey))
            {
                _logger.LogWarning("Square WebhookSignatureKey not configured - skipping signature verification");
                return Task.FromResult((true, (string?)null));
            }

            // Square docs indicate HMAC-SHA256 with key over requestUrl + body
            var payload = requestUrl + requestBody;
            var computed = ComputeHmacSha256Base64(_config.WebhookSignatureKey!, payload);

            var provided = signatureHeader?.Trim();

            var valid = SlowEquals(provided, computed);
            return Task.FromResult((valid, valid ? null : "Invalid Square webhook signature"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Square webhook signature");
            return Task.FromResult((false, ex.Message));
        }
    }

    private static string ComputeHmacSha256Base64(string secret, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return Convert.ToBase64String(hash);
    }

    private static bool SlowEquals(string? a, string? b)
    {
        if (a is null || b is null) return false;
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        if (aBytes.Length != bBytes.Length) return false;

        int diff = 0;
        for (int i = 0; i < aBytes.Length; i++)
        {
            diff |= aBytes[i] ^ bBytes[i];
        }
        return diff == 0;
    }

    // Hosted payment links via Square REST API (Payment Links)
    public async Task<(string? CheckoutUrl, DateTime? ExpiresAt, string? Error)> CreatePaymentLinkAsync(
        long amountMinor,
        string currency,
        string referenceId,
        string? email,
        CancellationToken ct)
    {
        try
        {
            // Choose correct domain based on environment
            var baseDomain = string.Equals(_config.Environment, "production", StringComparison.OrdinalIgnoreCase)
                ? "https://connect.squareup.com"
                : "https://connect.squareupsandbox.com";

            var url = $"{baseDomain}/v2/online-checkout/payment-links";

            // Build Quick Pay payload for Payment Links
            var body = new
            {
                idempotency_key = referenceId,
                quick_pay = new
                {
                    name = "Deposit",
                    price_money = new { amount = amountMinor, currency = currency },
                    location_id = string.IsNullOrWhiteSpace(_config.LocationId) ? null : _config.LocationId
                }
                // Optionally we could add "checkout_options" and "redirect_url" here
            };

            var http = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.AccessToken);
            req.Headers.TryAddWithoutValidation("Square-Version", "2023-10-18");
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var resp = await http.SendAsync(req, ct);
            var content = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Square Payment Link creation failed. Status: {Status}, Body: {Body}", resp.StatusCode, content);
                return (null, null, $"Square error {resp.StatusCode}");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            // Response example shape:
            // {
            //   "payment_link": { "id": "...", "url": "https://square.link/...", "created_at": "...", "expires_at": "...", ... }
            // }
            string? checkoutUrl = null;
            DateTime? expiresAt = null;

            if (root.TryGetProperty("payment_link", out var pl))
            {
                if (pl.TryGetProperty("url", out var urlEl) && urlEl.ValueKind == JsonValueKind.String)
                {
                    checkoutUrl = urlEl.GetString();
                }
                if (pl.TryGetProperty("expires_at", out var expEl) && expEl.ValueKind == JsonValueKind.String)
                {
                    if (DateTime.TryParse(expEl.GetString(), out var parsed)) expiresAt = parsed;
                }
            }

            if (string.IsNullOrEmpty(checkoutUrl))
            {
                // Some versions may return "checkout_url" - try fallback
                if (root.TryGetProperty("checkout_url", out var altUrl) && altUrl.ValueKind == JsonValueKind.String)
                {
                    checkoutUrl = altUrl.GetString();
                }
            }

            return (checkoutUrl, expiresAt, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePaymentLinkAsync unexpected error for Ref: {Ref}", referenceId);
            return (null, null, ex.Message);
        }
    }
}
