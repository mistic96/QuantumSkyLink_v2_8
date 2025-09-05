using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.DotsDev;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Services.Integrations;
using PaymentGatewayService.Services.Interfaces;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PaymentGatewayService.Controllers;

/// <summary>
/// Controller for Dots.dev global payout operations (190+ countries)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DotsDevController : ControllerBase
{
    private readonly IDotsDevService _dotsDevService;
    private readonly IPaymentService _paymentService;
    private readonly PaymentDbContext _context;
    private readonly ILogger<DotsDevController> _logger;

    public DotsDevController(
        IDotsDevService dotsDevService,
        IPaymentService paymentService,
        PaymentDbContext context,
        ILogger<DotsDevController> logger)
    {
        _dotsDevService = dotsDevService;
        _paymentService = paymentService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create an international payout
    /// </summary>
    /// <param name="request">Payout details with recipient information</param>
    /// <returns>Payout response with transaction details</returns>
    [HttpPost("payout")]
    [ProducesResponseType(typeof(DotsDevPayoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DotsDevPayoutResponse>> CreatePayout([FromBody] CreateDotsDevPayoutRequest request)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("User ID not found or invalid in token");
            }

            _logger.LogInformation("Creating Dots.dev payout for user {UserId}, amount {Amount} {Currency} to {Country}", 
                userId, request.Amount, request.Currency, request.RecipientCountry);

            // Find Dots.dev gateway
            var gateway = await _context.PaymentGateways
                .FirstOrDefaultAsync(g => g.GatewayType == PaymentGatewayType.DotsDev && g.IsActive);

            if (gateway == null)
            {
                return BadRequest("Dots.dev gateway is not configured or active");
            }

            // Create payment record
            var payment = new Payment
            {
                UserId = userId,
                PaymentGatewayId = gateway.Id,
                PaymentGateway = gateway.GatewayType, // Use PaymentGateway property
                Amount = request.Amount,
                Currency = request.Currency,
                Type = PaymentType.Withdrawal,
                Status = PaymentStatus.Pending,
                Metadata = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["recipientName"] = request.RecipientName,
                    ["recipientEmail"] = request.RecipientEmail,
                    ["recipientCountry"] = request.RecipientCountry,
                    ["recipientPhone"] = request.RecipientPhone ?? string.Empty,
                    ["paymentMethod"] = request.PreferredPaymentMethod ?? string.Empty,
                    ["description"] = request.Description ?? string.Empty
                })
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Prepare Dots.dev request
            var dotsDevRequest = new DotsDevPayoutRequest
            {
                AmountInCents = (int)(request.Amount * 100),
                Currency = request.Currency,
                IdempotencyKey = payment.Id.ToString(),
                Description = request.Description,
                Recipient = new DotsDevRecipient
                {
                    Name = request.RecipientName,
                    Email = request.RecipientEmail,
                    Country = request.RecipientCountry,
                    Phone = request.RecipientPhone,
                    PaymentMethod = request.PreferredPaymentMethod,
                    PaymentDetails = request.PaymentDetails,
                    ComplianceData = request.ComplianceData != null ? new DotsDevComplianceData
                    {
                        TaxId = request.ComplianceData.TaxId,
                        DateOfBirth = request.ComplianceData.DateOfBirth,
                        IdType = request.ComplianceData.IdType,
                        IdNumber = request.ComplianceData.IdNumber,
                        Address = request.ComplianceData.Address != null ? new DotsDevAddress
                        {
                            Line1 = request.ComplianceData.Address.Line1,
                            Line2 = request.ComplianceData.Address.Line2,
                            City = request.ComplianceData.Address.City,
                            State = request.ComplianceData.Address.State,
                            PostalCode = request.ComplianceData.Address.PostalCode,
                            Country = request.ComplianceData.Address.Country
                        } : null
                    } : null
                },
                Metadata = new Dictionary<string, object>
                {
                    ["paymentId"] = payment.Id.ToString(),
                    ["userId"] = userId.ToString()
                }
            };

            // Create payout through Dots.dev
            var result = await _dotsDevService.CreatePayoutAsync(dotsDevRequest);

            // Update payment with Dots.dev response
            payment.GatewayTransactionId = result.Id;
            payment.Status = result.Status.ToLower() switch
            {
                "completed" => PaymentStatus.Completed,
                "processing" => PaymentStatus.Processing,
                "failed" => PaymentStatus.Failed,
                _ => PaymentStatus.Pending
            };

            // Store additional details in metadata
            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new();
            metadata["dotsDevPaymentMethod"] = result.PaymentMethod;
            metadata["estimatedDelivery"] = result.EstimatedDelivery?.ToString("O") ?? string.Empty;
            if (result.Fees != null)
            {
                metadata["platformFee"] = result.Fees.PlatformFeeInCents;
                metadata["paymentMethodFee"] = result.Fees.PaymentMethodFeeInCents;
                metadata["totalFee"] = result.Fees.TotalFeeInCents;
                metadata["netAmount"] = result.Fees.NetAmountInCents;
            }
            payment.Metadata = JsonSerializer.Serialize(metadata);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Dots.dev payout created successfully. PaymentId: {PaymentId}, DotsDevId: {DotsDevId}, Status: {Status}", 
                payment.Id, result.Id, result.Status);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Dots.dev payout");
            return StatusCode(500, new { error = "Failed to create payout", message = ex.Message });
        }
    }

    /// <summary>
    /// Create an onboarding flow for recipient verification
    /// </summary>
    /// <param name="request">Flow creation request</param>
    /// <returns>Flow response with URL for user completion</returns>
    [HttpPost("flow")]
    [ProducesResponseType(typeof(DotsDevFlowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DotsDevFlowResponse>> CreateOnboardingFlow([FromBody] CreateDotsDevFlowRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation("Creating Dots.dev onboarding flow for user {UserId}, type {FlowType}", 
                userId, request.FlowType);

            var dotsDevRequest = new DotsDevFlowRequest
            {
                FlowType = request.FlowType,
                UserData = request.UserData,
                Metadata = new Dictionary<string, object>
                {
                    ["userId"] = userId,
                    ["requestedAt"] = DateTime.UtcNow.ToString("O")
                },
                RedirectUrl = request.RedirectUrl,
                Theme = request.Theme != null ? new DotsDevTheme
                {
                    PrimaryColor = request.Theme.PrimaryColor,
                    LogoUrl = request.Theme.LogoUrl,
                    CompanyName = request.Theme.CompanyName
                } : null
            };

            var result = await _dotsDevService.CreateOnboardingFlowAsync(dotsDevRequest);

            _logger.LogInformation("Dots.dev flow created successfully. FlowId: {FlowId}", result.FlowId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Dots.dev flow");
            return StatusCode(500, new { error = "Failed to create flow", message = ex.Message });
        }
    }

    /// <summary>
    /// Get the status of an onboarding flow
    /// </summary>
    /// <param name="flowId">Flow ID to check</param>
    /// <returns>Flow status and collected data</returns>
    [HttpGet("flow/{flowId}")]
    [ProducesResponseType(typeof(DotsDevFlowStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DotsDevFlowStatus>> GetFlowStatus(string flowId)
    {
        try
        {
            _logger.LogInformation("Getting Dots.dev flow status for flowId {FlowId}", flowId);

            var result = await _dotsDevService.GetFlowStatusAsync(flowId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Dots.dev flow status");
            return StatusCode(500, new { error = "Failed to get flow status", message = ex.Message });
        }
    }

    /// <summary>
    /// Get supported countries and their payment methods
    /// </summary>
    /// <returns>List of supported countries with available payment methods</returns>
    [HttpGet("countries")]
    [ProducesResponseType(typeof(List<DotsDevCountrySupport>), StatusCodes.Status200OK)]
    [AllowAnonymous] // Public endpoint
    public async Task<ActionResult<List<DotsDevCountrySupport>>> GetSupportedCountries()
    {
        try
        {
            _logger.LogInformation("Getting Dots.dev supported countries");

            var result = await _dotsDevService.GetSupportedCountriesAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Dots.dev supported countries");
            return StatusCode(500, new { error = "Failed to get supported countries", message = ex.Message });
        }
    }

    /// <summary>
    /// Get payment methods available for a specific country
    /// </summary>
    /// <param name="country">Country code (US, GB, IN, PH, etc.)</param>
    /// <returns>Available payment methods for the country</returns>
    [HttpGet("methods/{country}")]
    [ProducesResponseType(typeof(DotsDevCountrySupport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous] // Public endpoint
    public async Task<ActionResult<DotsDevCountrySupport>> GetCountryPaymentMethods(string country)
    {
        try
        {
            _logger.LogInformation("Getting payment methods for country {Country}", country);

            var countries = await _dotsDevService.GetSupportedCountriesAsync();
            var countrySupport = countries.FirstOrDefault(c => 
                c.CountryCode.Equals(country, StringComparison.OrdinalIgnoreCase));

            if (countrySupport == null)
            {
                return NotFound(new { error = $"Country {country} is not supported" });
            }

            return Ok(countrySupport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment methods for country {Country}", country);
            return StatusCode(500, new { error = "Failed to get payment methods", message = ex.Message });
        }
    }

    /// <summary>
    /// Validate recipient details for a specific country
    /// </summary>
    /// <param name="request">Validation request with country and recipient data</param>
    /// <returns>Validation result with any errors</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(DotsDevValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DotsDevValidationResult>> ValidateRecipient([FromBody] ValidateRecipientRequest request)
    {
        try
        {
            _logger.LogInformation("Validating recipient for country {Country}", request.Country);

            var result = await _dotsDevService.ValidateRecipientAsync(request.Country, request.RecipientData);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating recipient");
            return StatusCode(500, new { error = "Failed to validate recipient", message = ex.Message });
        }
    }
}

#region Request DTOs

public class CreateDotsDevPayoutRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientCountry { get; set; } = string.Empty;
    public string? RecipientPhone { get; set; }
    public string? PreferredPaymentMethod { get; set; }
    public Dictionary<string, object>? PaymentDetails { get; set; }
    public string? Description { get; set; }
    public ComplianceDataRequest? ComplianceData { get; set; }
}

public class ComplianceDataRequest
{
    public string? TaxId { get; set; }
    public string? DateOfBirth { get; set; }
    public string? IdType { get; set; }
    public string? IdNumber { get; set; }
    public AddressRequest? Address { get; set; }
}

public class AddressRequest
{
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class CreateDotsDevFlowRequest
{
    public string FlowType { get; set; } = "payout_onboarding";
    public Dictionary<string, object>? UserData { get; set; }
    public string? RedirectUrl { get; set; }
    public ThemeRequest? Theme { get; set; }
}

public class ThemeRequest
{
    public string? PrimaryColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? CompanyName { get; set; }
}

public class ValidateRecipientRequest
{
    public string Country { get; set; } = string.Empty;
    public Dictionary<string, object> RecipientData { get; set; } = new();
}

#endregion
