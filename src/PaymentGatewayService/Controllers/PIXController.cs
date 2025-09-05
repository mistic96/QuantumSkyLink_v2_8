using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.PIXBrazil;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Services.Integrations;
using PaymentGatewayService.Services.Interfaces;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PaymentGatewayService.Controllers;

/// <summary>
/// Controller for PIX Brazil payment operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PIXController : ControllerBase
{
    private readonly IPIXBrazilService _pixService;
    private readonly IPaymentService _paymentService;
    private readonly PaymentDbContext _context;
    private readonly ILogger<PIXController> _logger;

    public PIXController(
        IPIXBrazilService pixService,
        IPaymentService paymentService,
        PaymentDbContext context,
        ILogger<PIXController> logger)
    {
        _pixService = pixService;
        _paymentService = paymentService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a PIX charge (payment request) with QR code
    /// </summary>
    /// <param name="request">PIX charge details</param>
    /// <returns>PIX charge response with QR code</returns>
    [HttpPost("charge")]
    [ProducesResponseType(typeof(PIXChargeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PIXChargeResponse>> CreateCharge([FromBody] CreatePIXChargeRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Creating PIX charge for user {UserId}, amount {Amount}", userId, request.AmountInCents);

            // Get PIX gateway info
            var pixGateway = await _context.PaymentGateways
                .FirstOrDefaultAsync(g => g.GatewayType == PaymentGatewayType.PIXBrazil && g.IsActive);
            
            if (pixGateway == null)
            {
                throw new InvalidOperationException("PIX Brazil gateway not configured");
            }

            // Create payment record
            var payment = new Payment
            {
                UserId = userId, // String, compatible with Logto user IDs
                Amount = request.AmountInCents / 100m,
                Currency = "BRL",
                Type = PaymentType.Deposit,
                Status = PaymentStatus.Pending,
                PaymentGatewayId = pixGateway.Id,
                PaymentGateway = pixGateway.GatewayType, // Use PaymentGateway property
                Description = request.Description,
                Metadata = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["payerName"] = request.PayerName,
                    ["payerDocument"] = request.PayerDocument,
                    ["payerEmail"] = request.PayerEmail ?? string.Empty,
                    ["payerPhone"] = request.PayerPhone ?? string.Empty
                })
            };

            // Add custom metadata if provided
            if (request.Metadata != null)
            {
                var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata) ?? new Dictionary<string, object>();
                foreach (var kvp in request.Metadata)
                {
                    metadata[$"custom_{kvp.Key}"] = kvp.Value;
                }
                payment.Metadata = JsonSerializer.Serialize(metadata);
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Create PIX charge
            var pixRequest = new PIXChargeRequest
            {
                Amount = request.AmountInCents,
                Currency = "BRL",
                PaymentMethod = "PIX_DYNAMIC_QR",
                Description = request.Description,
                ExpirationInfo = request.ExpirationSeconds.HasValue 
                    ? new PIXExpirationInfo { Seconds = request.ExpirationSeconds.Value }
                    : new PIXExpirationInfo { Seconds = 3600 },
                Payer = new PIXPayer
                {
                    Name = request.PayerName,
                    Document = request.PayerDocument,
                    Email = request.PayerEmail,
                    Phone = request.PayerPhone
                },
                CallbackUrl = request.CallbackUrl ?? $"{Request.Scheme}://{Request.Host}/api/webhook/pix/liquido"
            };

            var pixResponse = await _pixService.CreateChargeAsync(pixRequest);

            // Update payment with gateway transaction ID
            payment.GatewayTransactionId = pixResponse.Id;
            var chargeMetadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata) ?? new Dictionary<string, object>();
            chargeMetadata["pixQRCode"] = pixResponse.QRCode ?? string.Empty;
            chargeMetadata["pixQRCodeBase64"] = pixResponse.QRCodeBase64 ?? string.Empty;
            payment.Metadata = JsonSerializer.Serialize(chargeMetadata);
            await _context.SaveChangesAsync();

            // Map to API response
            return Ok(new PIXChargeResponse
            {
                ChargeId = payment.Id.ToString(),
                Status = pixResponse.Status,
                QRCodeString = pixResponse.QRCode ?? string.Empty,
                QRCodeImage = pixResponse.QRCodeBase64 ?? string.Empty,
                AmountInCents = request.AmountInCents,
                Currency = "BRL",
                ExpiresAt = pixResponse.ExpiresAt ?? DateTime.UtcNow.AddHours(1),
                CreatedAt = payment.CreatedAt,
                Description = request.Description,
                Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PIX charge");
            return BadRequest(new { error = "Failed to create PIX charge", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a PIX payout (send money)
    /// </summary>
    /// <param name="request">PIX payout details</param>
    /// <returns>PIX payout response</returns>
    [HttpPost("payout")]
    [ProducesResponseType(typeof(PIXPayoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PIXPayoutResponse>> CreatePayout([FromBody] CreatePIXPayoutRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Creating PIX payout for user {UserId}, amount {Amount}", userId, request.AmountInCents);

            // Verify user has sufficient balance
            // This would typically involve checking account balance
            // For now, we'll proceed with the payout

            // Get PIX gateway info
            var pixGateway = await _context.PaymentGateways
                .FirstOrDefaultAsync(g => g.GatewayType == PaymentGatewayType.PIXBrazil && g.IsActive);
            
            if (pixGateway == null)
            {
                throw new InvalidOperationException("PIX Brazil gateway not configured");
            }

            // Create payment record
            var payment = new Payment
            {
                UserId = userId, // String, compatible with Logto user IDs
                Amount = request.AmountInCents / 100m,
                Currency = "BRL",
                Type = PaymentType.Withdrawal,
                Status = PaymentStatus.Pending,
                PaymentGatewayId = pixGateway.Id,
                PaymentGateway = pixGateway.GatewayType, // Use PaymentGateway property
                Description = request.Description,
                Metadata = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["pixKey"] = request.PixKey,
                    ["pixKeyType"] = request.PixKeyType,
                    ["targetName"] = request.RecipientName,
                    ["targetDocument"] = request.RecipientDocument ?? string.Empty
                })
            };

            // Add custom metadata if provided
            if (request.Metadata != null)
            {
                var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata) ?? new Dictionary<string, object>();
                foreach (var kvp in request.Metadata)
                {
                    metadata[$"custom_{kvp.Key}"] = kvp.Value;
                }
                payment.Metadata = JsonSerializer.Serialize(metadata);
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Create PIX payout
            var pixRequest = new PIXPayoutRequest
            {
                AmountInCents = request.AmountInCents,
                Currency = "BRL",
                TargetPixKey = request.PixKey,
                TargetPixKeyType = request.PixKeyType,
                TargetName = request.RecipientName,
                TargetDocument = request.RecipientDocument,
                Description = request.Description,
                IdempotencyKey = request.IdempotencyKey ?? payment.Id.ToString()
            };

            var pixResponse = await _pixService.CreatePayoutAsync(pixRequest);

            // Update payment with gateway transaction ID
            payment.GatewayTransactionId = pixResponse.Id;
            payment.Status = pixResponse.Status.ToLower() switch
            {
                "completed" => PaymentStatus.Completed,
                "processing" => PaymentStatus.Processing,
                "failed" => PaymentStatus.Failed,
                _ => PaymentStatus.Pending
            };
            
            if (pixResponse.EndToEndId != null)
            {
                var payoutMetadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata) ?? new Dictionary<string, object>();
                payoutMetadata["endToEndId"] = pixResponse.EndToEndId;
                payment.Metadata = JsonSerializer.Serialize(payoutMetadata);
            }
            
            await _context.SaveChangesAsync();

            // Map to API response
            return Ok(new PIXPayoutResponse
            {
                PayoutId = payment.Id.ToString(),
                Status = pixResponse.Status,
                AmountInCents = request.AmountInCents,
                Currency = "BRL",
                RecipientPixKey = request.PixKey,
                RecipientName = request.RecipientName,
                EndToEndId = pixResponse.EndToEndId,
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.Status == PaymentStatus.Completed ? DateTime.UtcNow : null,
                Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PIX payout");
            return BadRequest(new { error = "Failed to create PIX payout", details = ex.Message });
        }
    }

    /// <summary>
    /// Get PIX transaction status
    /// </summary>
    /// <param name="transactionId">Payment ID or Gateway Transaction ID</param>
    /// <returns>Transaction status details</returns>
    [HttpGet("transaction/{transactionId}")]
    [ProducesResponseType(typeof(PIXTransactionStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PIXTransactionStatusResponse>> GetTransactionStatus(string transactionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Try to find payment by ID or gateway transaction ID
            Payment? payment = null;
            
            if (Guid.TryParse(transactionId, out var paymentId))
            {
                payment = await _context.Payments
                    .Include(p => p.PaymentGateway)
                    .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);
            }
            
            if (payment == null)
            {
                payment = await _context.Payments
                    .Include(p => p.PaymentGateway)
                    .FirstOrDefaultAsync(p => p.GatewayTransactionId == transactionId && p.UserId == userId);
            }

            if (payment == null || payment.PaymentGateway != PaymentGatewayType.PIXBrazil)
            {
                return NotFound(new { error = "PIX transaction not found" });
            }

            // Get latest status from PIX provider
            if (!string.IsNullOrEmpty(payment.GatewayTransactionId))
            {
                var pixStatus = await _pixService.GetTransactionStatusAsync(payment.GatewayTransactionId);
                
                // Update payment status if changed
                var newStatus = pixStatus.Status.ToLower() switch
                {
                    "completed" => PaymentStatus.Completed,
                    "processing" => PaymentStatus.Processing,
                    "failed" => PaymentStatus.Failed,
                    "cancelled" => PaymentStatus.Cancelled,
                    _ => PaymentStatus.Pending
                };

                if (payment.Status != newStatus)
                {
                    payment.Status = newStatus;
                    await _context.SaveChangesAsync();
                }
            }

            // Build response
            var response = new PIXTransactionStatusResponse
            {
                TransactionId = payment.Id.ToString(),
                Type = payment.Type == PaymentType.Deposit ? "charge" : "payout",
                Status = payment.Status.ToString().ToLower(),
                AmountInCents = (int)(payment.Amount * 100),
                Currency = payment.Currency,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt ?? payment.CreatedAt
            };

            // Add QR code data for charges
            if (payment.Type == PaymentType.Deposit && payment.Metadata != null)
            {
                var statusMetadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata);
                var qrCode = statusMetadata?.GetValueOrDefault("pixQRCode")?.ToString();
                var qrCodeBase64 = statusMetadata?.GetValueOrDefault("pixQRCodeBase64")?.ToString();
                
                if (!string.IsNullOrEmpty(qrCode))
                {
                    response.QRCode = new PIXQRCodeData
                    {
                        QRCodeString = qrCode,
                        QRCodeImage = qrCodeBase64 ?? string.Empty,
                        ExpiresAt = payment.CreatedAt.AddHours(1) // Default expiration
                    };
                }
            }

            // Add payer/recipient info
            if (payment.Metadata != null)
            {
                var infoMetadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata);
                if (payment.Type == PaymentType.Deposit && payment.Status == PaymentStatus.Completed)
                {
                    response.Payer = new PIXPayerInfo
                    {
                        Name = infoMetadata?.GetValueOrDefault("payerName")?.ToString() ?? string.Empty,
                        Document = infoMetadata?.GetValueOrDefault("payerDocument")?.ToString() ?? string.Empty
                    };
                }
                else if (payment.Type == PaymentType.Withdrawal)
                {
                    response.Recipient = new PIXRecipientInfo
                    {
                        Name = infoMetadata?.GetValueOrDefault("targetName")?.ToString() ?? string.Empty,
                        PixKey = infoMetadata?.GetValueOrDefault("pixKey")?.ToString() ?? string.Empty,
                        PixKeyType = infoMetadata?.GetValueOrDefault("pixKeyType")?.ToString() ?? string.Empty,
                        Document = infoMetadata?.GetValueOrDefault("targetDocument")?.ToString()
                    };
                }
            }

            // Add transaction events
            response.Events.Add(new PIXTransactionEvent
            {
                EventType = "created",
                Description = "Transaction created",
                OccurredAt = payment.CreatedAt
            });

            if (payment.Status == PaymentStatus.Completed && payment.UpdatedAt.HasValue)
            {
                response.Events.Add(new PIXTransactionEvent
                {
                    EventType = "completed",
                    Description = "Transaction completed successfully",
                    OccurredAt = payment.UpdatedAt.Value
                });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PIX transaction status");
            return StatusCode(500, new { error = "Failed to get transaction status" });
        }
    }

    /// <summary>
    /// Generate a static PIX QR code
    /// </summary>
    /// <param name="request">Static QR code details</param>
    /// <returns>Static QR code response</returns>
    [HttpPost("qrcode/static")]
    [ProducesResponseType(typeof(PIXStaticQRCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PIXStaticQRCodeResponse>> GenerateStaticQRCode([FromBody] GenerateStaticQRCodeRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Generating static PIX QR code for user {UserId}", userId);

            var pixResponse = await _pixService.GenerateStaticQRCodeAsync(
                request.AmountInCents,
                request.Description ?? string.Empty);

            return Ok(new PIXStaticQRCodeResponse
            {
                QRCodeId = pixResponse.Id,
                QRCodeString = pixResponse.QRCode,
                QRCodeImage = pixResponse.QRCodeBase64,
                AmountInCents = request.AmountInCents,
                MerchantName = request.MerchantName,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                Metadata = request.Metadata?.ToDictionary(k => k.Key, v => (object)v.Value)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating static PIX QR code");
            return BadRequest(new { error = "Failed to generate QR code", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate a PIX key
    /// </summary>
    /// <param name="request">PIX key validation request</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate-key")]
    [ProducesResponseType(typeof(ValidatePixKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ValidatePixKeyResponse>> ValidatePixKey([FromBody] ValidatePixKeyRequest request)
    {
        try
        {
            _logger.LogInformation("Validating PIX key type {KeyType}", request.KeyType);

            var isValid = await _pixService.ValidatePixKeyAsync(request.PixKey, request.KeyType);

            var response = new ValidatePixKeyResponse
            {
                IsValid = isValid,
                PixKey = request.PixKey,
                KeyType = request.KeyType,
                Message = isValid ? "PIX key is valid" : "PIX key is invalid"
            };

            if (!isValid)
            {
                response.Details = new Dictionary<string, string>
                {
                    ["reason"] = request.KeyType.ToLowerInvariant() switch
                    {
                        "cpf" or "cnpj" => "Invalid document format",
                        "email" => "Invalid email format",
                        "phone" => "Invalid phone number format",
                        "random" => "Invalid UUID format",
                        _ => "Unknown key type"
                    }
                };
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating PIX key");
            return BadRequest(new { error = "Failed to validate PIX key", details = ex.Message });
        }
    }

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var gid))
            throw new UnauthorizedAccessException("User ID not found or invalid in claims");
        return gid;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    private async Task<Guid> GetPIXGatewayId()
    {
        var pixGateway = await _context.PaymentGateways
            .FirstOrDefaultAsync(g => g.GatewayType == PaymentGatewayType.PIXBrazil && g.IsActive);

        if (pixGateway == null)
        {
            throw new InvalidOperationException("PIX Brazil gateway not configured");
        }

        return pixGateway.Id;
    }

    #endregion
}
