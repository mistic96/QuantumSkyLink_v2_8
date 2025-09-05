using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaymentGatewayService.Configuration;
using PaymentGatewayService.Services.Integrations;
using System.Security.Claims;
using System.Net.Http.Json;

namespace PaymentGatewayService.Controllers;

[ApiController]
[Route("api/providers")]
[Authorize] // MobileAPIGateway will call this internally
public class ProvidersController : ControllerBase
{
    private readonly IOptions<SquareConfiguration> _squareOptions;
    private readonly ISquareService _squareService;
    private readonly ILogger<ProvidersController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ProvidersController(
        IOptions<SquareConfiguration> squareOptions,
        ISquareService squareService,
        ILogger<ProvidersController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _squareOptions = squareOptions;
        _squareService = squareService;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Returns Square client parameters the mobile app needs to initialize Square SDK.
    /// Only exposes non-secret values (e.g., applicationId).
    /// </summary>
    [HttpGet("square/client-params")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetSquareClientParams([FromQuery] decimal amount, [FromQuery] string currency = "USD", [FromQuery] string? referenceId = null)
    {
        var cfg = _squareOptions.Value;

        var amountMinor = PaymentGatewayService.Models.Square.MoneyConverter.ToMinorUnits(amount, currency);

        var result = new Dictionary<string, object>
        {
            ["applicationId"] = cfg.ApplicationId,
            ["environment"] = string.Equals(cfg.Environment, "production", StringComparison.OrdinalIgnoreCase) ? "production" : "sandbox",
            ["locationId"] = cfg.LocationId ?? string.Empty,
            ["amountMoney"] = new Dictionary<string, object> { ["amount"] = amountMinor, ["currency"] = currency },
            ["referenceId"] = referenceId ?? Guid.NewGuid().ToString()
        };

        return Ok(result);
    }

    /// <summary>
    /// Creates a Square hosted payment link (Payment Links) for the given amount and returns the checkout URL.
    /// Optionally accepts a buyer email for out-of-band delivery (email sending can be handled by a separate service).
    /// </summary>
    [HttpPost("square/payment-link")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSquarePaymentLink([FromBody] CreatePaymentLinkDto request, CancellationToken ct)
    {
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.Currency))
        {
            return BadRequest(new { error = "Invalid amount or currency" });
        }

        var amountMinor = PaymentGatewayService.Models.Square.MoneyConverter.ToMinorUnits(request.Amount, request.Currency);
        var referenceId = string.IsNullOrWhiteSpace(request.ReferenceId) ? Guid.NewGuid().ToString() : request.ReferenceId;

        var (checkoutUrl, expiresAt, error) = await _squareService.CreatePaymentLinkAsync(
            amountMinor, request.Currency, referenceId, request.Email, ct);

        if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(checkoutUrl))
        {
            _logger.LogWarning("Failed to create Square payment link. Ref: {Ref}, Error: {Error}", referenceId, error);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = error ?? "Failed to create payment link" });
        }

        // Attempt to email the link via NotificationService if an email was provided.
        var emailSent = false;
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            try
            {
                var client = _httpClientFactory.CreateClient("NotificationService");

                // Forward the incoming Authorization header (user token) so NotificationService can authorize on behalf of the user
                if (Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(authHeader.ToString());
                }

                // Extract current user ID from claims to satisfy NotificationService access control
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    var payload = new
                    {
                        UserId = userId,
                        Type = "Email",
                        Subject = "Your payment link",
                        Body = $"Complete your deposit using this Square payment link: {checkoutUrl}\nReference: {referenceId}\nExpires: {(expiresAt.HasValue ? expiresAt.Value.ToString("u") : "N/A")}",
                        HtmlBody = $"<p>Complete your deposit using this Square payment link: <a href=\"{checkoutUrl}\">Pay with Square</a></p><p>Reference: {referenceId}</p><p>Expires: {(expiresAt.HasValue ? expiresAt.Value.ToString("u") : "N/A")}</p>",
                        Recipient = request.Email,
                        Variables = new Dictionary<string, object>
                        {
                            ["provider"] = "SQUARE",
                            ["referenceId"] = referenceId,
                            ["amount"] = request.Amount,
                            ["currency"] = request.Currency,
                            ["checkoutUrl"] = checkoutUrl
                        },
                        Metadata = new Dictionary<string, object>
                        {
                            ["provider"] = "SQUARE",
                            ["captureMode"] = "payment_link"
                        }
                    };

                    var resp = await client.PostAsJsonAsync("/api/notification", payload, ct);
                    emailSent = resp.IsSuccessStatusCode;
                }
                else
                {
                    _logger.LogWarning("Could not parse user ID from claims; skipping email send for payment link Ref: {Ref}", referenceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification email for payment link Ref: {Ref}", referenceId);
            }
        }

        return Ok(new
        {
            checkoutUrl,
            expiresAt,
            referenceId,
            emailSent
        });
    }

    public class CreatePaymentLinkDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string? ReferenceId { get; set; }
        public string? Email { get; set; }
        public string? Note { get; set; }
    }
}
