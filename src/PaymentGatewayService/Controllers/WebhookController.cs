using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.PIXBrazil;
using PaymentGatewayService.Models.DotsDev;
using PaymentGatewayService.Models.MoonPay;
using PaymentGatewayService.Models.Coinbase;
using PaymentGatewayService.Services.Integrations;
using PaymentGatewayService.Utils;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace PaymentGatewayService.Controllers;

/// <summary>
/// Controller for handling payment provider webhooks
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Webhooks must be accessible without authentication
public class WebhookController : ControllerBase
{
    private readonly IPIXBrazilService _pixService;
    private readonly PaymentDbContext _context;
    private readonly ILogger<WebhookController> _logger;
    private readonly IConfiguration _configuration;
    private readonly PaymentGatewayService.Services.SquareWebhookService _squareWebhookService;

    public WebhookController(
        IPIXBrazilService pixService,
        PaymentDbContext context,
        ILogger<WebhookController> logger,
        IConfiguration configuration,
        PaymentGatewayService.Services.SquareWebhookService squareWebhookService)
    {
        _pixService = pixService;
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _squareWebhookService = squareWebhookService;
    }

    /// <summary>
    /// Handle PIX webhook notifications
    /// </summary>
    /// <param name="provider">PIX provider name (liquido, ebanx, cielo)</param>
    /// <returns>Webhook acknowledgment</returns>
    [HttpPost("pix/{provider}")]
    [Consumes("application/json")]
    public async Task<IActionResult> HandlePIXWebhook(string provider)
    {
        try
        {
            // Read raw body for signature verification
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            _logger.LogInformation("Received PIX webhook from provider {Provider}, body length: {Length}", 
                provider, rawBody.Length);

            // Log webhook for debugging (remove in production)
            _logger.LogDebug("PIX webhook body: {Body}", rawBody);

            // Parse webhook payload
            var payload = JsonSerializer.Deserialize<PIXWebhookPayload>(rawBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null)
            {
                _logger.LogWarning("Failed to parse PIX webhook payload");
                return BadRequest(new { error = "Invalid payload" });
            }

            // Store webhook record
            var webhook = new PaymentWebhook
            {
                GatewayType = PaymentGatewayType.PIXBrazil,
                EventType = payload.Type ?? "unknown",
                ExternalEventId = payload.EventId,
                Payload = rawBody,
                Status = WebhookStatus.Pending,
                ProcessedAt = null,
                Signature = payload.Signature,
                SourceIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Headers = JsonSerializer.Serialize(Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()))
            };

            _context.PaymentWebhooks.Add(webhook);
            await _context.SaveChangesAsync();

            // Process webhook with PIX service
            var processed = await _pixService.ProcessWebhookAsync(payload);

            if (!processed)
            {
                webhook.Status = WebhookStatus.Failed;
                webhook.ErrorMessage = "Signature verification failed or invalid timestamp";
                await _context.SaveChangesAsync();
                
                _logger.LogWarning("PIX webhook processing failed for provider {Provider}", provider);
                return Unauthorized(new { error = "Invalid signature or timestamp" });
            }

            // Find and update the payment
            if (payload.Data != null && !string.IsNullOrEmpty(payload.Data.Id))
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.GatewayTransactionId == payload.Data.Id);

                if (payment != null)
                {
                    webhook.RelatedPaymentId = payment.Id;
                    
                    // Update payment status based on webhook event
                    var previousStatus = payment.Status;
                    
                    switch (payload.Type?.ToLower())
                    {
                        case "payment.completed":
                        case "payment.success":
                        case "payout.completed":
                            payment.Status = PaymentStatus.Completed;
                            _logger.LogInformation("PIX payment {PaymentId} completed", payment.Id);
                            break;
                            
                        case "payment.failed":
                        case "payout.failed":
                            payment.Status = PaymentStatus.Failed;
                            // Store failure reason in metadata
                            var failureMetadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                            failureMetadata["failureReason"] = payload.Data.Metadata?.GetValueOrDefault("error")?.ToString() ?? "Payment failed";
                            payment.Metadata = JsonSerializer.Serialize(failureMetadata);
                            _logger.LogWarning("PIX payment {PaymentId} failed", payment.Id);
                            break;
                            
                        case "payment.cancelled":
                        case "payout.cancelled":
                            payment.Status = PaymentStatus.Cancelled;
                            _logger.LogInformation("PIX payment {PaymentId} cancelled", payment.Id);
                            break;
                            
                        case "payment.processing":
                        case "payout.processing":
                            payment.Status = PaymentStatus.Processing;
                            break;
                    }

                    // Store additional webhook data in payment metadata
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                    metadata[$"webhook_{payload.Type}"] = DateTime.UtcNow.ToString("O");
                    
                    if (payload.Data.EndToEndId != null)
                    {
                        metadata["endToEndId"] = payload.Data.EndToEndId;
                    }
                    
                    payment.Metadata = JsonSerializer.Serialize(metadata);

                    // Create payment attempt record if status changed
                    if (previousStatus != payment.Status)
                    {
                        var attempt = new PaymentAttempt
                        {
                            PaymentId = payment.Id,
                            Status = payment.Status switch
                            {
                                PaymentStatus.Completed => PaymentAttemptStatus.Succeeded,
                                PaymentStatus.Failed => PaymentAttemptStatus.Failed,
                                PaymentStatus.Cancelled => PaymentAttemptStatus.Cancelled,
                                PaymentStatus.Processing => PaymentAttemptStatus.Processing,
                                _ => PaymentAttemptStatus.Pending
                            },
                            GatewayResponse = JsonSerializer.Serialize(payload.Data),
                            AttemptNumber = 1,
                            Amount = payment.Amount,
                            Currency = payment.Currency,
                            ProcessedAt = DateTime.UtcNow,
                            ProcessingTimeMs = 0
                        };
                        
                        _context.PaymentAttempts.Add(attempt);
                    }
                }
                else
                {
                    _logger.LogWarning("Payment not found for PIX transaction {TransactionId}", payload.Data.Id);
                }
            }

            // Mark webhook as processed
            webhook.ProcessedAt = DateTime.UtcNow;
            webhook.Status = WebhookStatus.Processed;
            await _context.SaveChangesAsync();

            _logger.LogInformation("PIX webhook processed successfully for provider {Provider}", provider);
            return Ok(new { status = "success", received = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PIX webhook from provider {Provider}", provider);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Handle Stripe webhook notifications
    /// </summary>
    /// <returns>Webhook acknowledgment</returns>
    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        try
        {
            // Read raw body
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // Get Stripe signature header
            var signature = Request.Headers["Stripe-Signature"].ToString();
            
            _logger.LogInformation("Received Stripe webhook, signature present: {HasSignature}", !string.IsNullOrEmpty(signature));

            // TODO: Implement Stripe webhook signature verification
            // TODO: Parse Stripe event and update payment status

            // For now, just acknowledge receipt
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Handle Square webhook notifications
    /// </summary>
    /// <returns>Webhook acknowledgment</returns>
    [HttpPost("square")]
    public async Task<IActionResult> HandleSquareWebhook()
    {
        try
        {
            // Read raw body
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // Compute full request URL for signature verification
            var requestUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";

            // Get Square signature header
            var signature = Request.Headers["X-Square-Signature"].ToString();
            _logger.LogInformation("Received Square webhook, signature present: {HasSignature}", !string.IsNullOrEmpty(signature));

            // Store webhook record (pending)
            var webhook = new PaymentWebhook
            {
                GatewayType = PaymentGatewayType.Square,
                EventType = "unknown",
                ExternalEventId = null,
                Payload = rawBody,
                Status = WebhookStatus.Pending,
                ProcessedAt = null,
                Signature = signature,
                SourceIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Headers = JsonSerializer.Serialize(Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()))
            };

            _context.PaymentWebhooks.Add(webhook);
            await _context.SaveChangesAsync();

            // Verify signature
            var (valid, error) = await _squareWebhookService.VerifyAsync(signature, rawBody, requestUrl, HttpContext.RequestAborted);
            if (!valid)
            {
                webhook.Status = WebhookStatus.Failed;
                webhook.ErrorMessage = $"Signature verification failed: {error}";
                await _context.SaveChangesAsync();
                _logger.LogWarning("Square webhook signature verification failed");
                return Unauthorized(new { error = "Invalid signature" });
            }

            // Parse payload
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            string? eventType = root.TryGetProperty("type", out var typeEl) ? typeEl.GetString() : null;
            webhook.EventType = eventType ?? "unknown";

            // Extract payment object if present
            string? gatewayPaymentId = null;
            string? squarePaymentStatus = null;

            if (root.TryGetProperty("data", out var dataEl) &&
                dataEl.TryGetProperty("object", out var objectEl))
            {
                // Typical path: data.object.payment
                if (objectEl.TryGetProperty("payment", out var paymentEl))
                {
                    if (paymentEl.TryGetProperty("id", out var idEl)) gatewayPaymentId = idEl.GetString();
                    if (paymentEl.TryGetProperty("status", out var statusEl)) squarePaymentStatus = statusEl.GetString();
                }
                else
                {
                    // Fallback: data.object.id/status
                    if (objectEl.TryGetProperty("id", out var id2)) gatewayPaymentId = id2.GetString();
                    if (objectEl.TryGetProperty("status", out var st2)) squarePaymentStatus = st2.GetString();
                }
            }

            // Update related payment if found
            if (!string.IsNullOrEmpty(gatewayPaymentId))
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.GatewayTransactionId == gatewayPaymentId);
                if (payment != null)
                {
                    webhook.RelatedPaymentId = payment.Id;

                    var previousStatus = payment.Status;
                    var mappedModelStatus = await _squareWebhookService.MapEventToStatusAsync(eventType, squarePaymentStatus);
                    // MapEventToStatusAsync already returns a Data.Entities.PaymentStatus
                    payment.Status = mappedModelStatus;

                    // Store webhook event in metadata
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                    metadata[$"webhook_{eventType ?? "square_event"}"] = DateTime.UtcNow.ToString("O");
                    if (!string.IsNullOrEmpty(squarePaymentStatus))
                    {
                        metadata["squarePaymentStatus"] = squarePaymentStatus;
                    }
                    payment.Metadata = JsonSerializer.Serialize(metadata);

                    // Create payment attempt record if status changed
                    if (previousStatus != payment.Status)
                    {
                        var attempt = new PaymentAttempt
                        {
                            PaymentId = payment.Id,
                            Status = payment.Status switch
                            {
                                PaymentStatus.Completed => PaymentAttemptStatus.Succeeded,
                                PaymentStatus.Failed => PaymentAttemptStatus.Failed,
                                PaymentStatus.Cancelled => PaymentAttemptStatus.Cancelled,
                                PaymentStatus.Processing => PaymentAttemptStatus.Processing,
                                PaymentStatus.Pending => PaymentAttemptStatus.Pending,
                                _ => PaymentAttemptStatus.Pending
                            },
                            GatewayResponse = rawBody,
                            AttemptNumber = 1,
                            Amount = payment.Amount,
                            Currency = payment.Currency,
                            ProcessedAt = DateTime.UtcNow,
                            ProcessingTimeMs = 0
                        };
                        _context.PaymentAttempts.Add(attempt);
                    }

                    // If completed, set CompletedAt
                    if (payment.Status == PaymentStatus.Completed && payment.CompletedAt == null)
                    {
                        payment.CompletedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning("Square payment not found for GatewayTransactionId {GatewayTransactionId}", gatewayPaymentId);
                }
            }

            // Mark webhook as processed
            webhook.ProcessedAt = DateTime.UtcNow;
            webhook.Status = WebhookStatus.Processed;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Square webhook processed successfully, eventType: {EventType}", eventType);
            return Ok(new { status = "success", received = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Square webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Handle Dots.dev webhook notifications
    /// </summary>
    /// <returns>Webhook acknowledgment</returns>
    [HttpPost("dotsdev")]
    public async Task<IActionResult> HandleDotsDevWebhook()
    {
        try
        {
            // Read raw body
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // Get Dots.dev signature header
            var signature = Request.Headers["X-DotsDev-Signature"].ToString();
            
            _logger.LogInformation("Received Dots.dev webhook, signature present: {HasSignature}", !string.IsNullOrEmpty(signature));

            // Parse webhook payload
            var payload = JsonSerializer.Deserialize<DotsDevWebhookPayload>(rawBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null)
            {
                _logger.LogWarning("Failed to parse Dots.dev webhook payload");
                return BadRequest(new { error = "Invalid payload" });
            }

            // Store webhook record
            var webhook = new PaymentWebhook
            {
                GatewayType = PaymentGatewayType.DotsDev,
                EventType = payload.Event ?? "unknown",
                ExternalEventId = payload.EventId,
                Payload = rawBody,
                Status = WebhookStatus.Pending,
                ProcessedAt = null,
                Signature = signature,
                SourceIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Headers = JsonSerializer.Serialize(Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()))
            };

            _context.PaymentWebhooks.Add(webhook);
            await _context.SaveChangesAsync();

            // Process webhook with Dots.dev service
            var dotsDevService = HttpContext.RequestServices.GetService<IDotsDevService>();
            if (dotsDevService == null)
            {
                _logger.LogError("Dots.dev service not found");
                webhook.Status = WebhookStatus.Failed;
                webhook.ErrorMessage = "Dots.dev service not configured";
                await _context.SaveChangesAsync();
                return StatusCode(500, new { error = "Service configuration error" });
            }

            var processed = await dotsDevService.ProcessWebhookAsync(payload, signature);

            if (!processed)
            {
                webhook.Status = WebhookStatus.Failed;
                webhook.ErrorMessage = "Signature verification failed";
                await _context.SaveChangesAsync();
                
                _logger.LogWarning("Dots.dev webhook processing failed");
                return Unauthorized(new { error = "Invalid signature" });
            }

            // Update payment based on webhook event
            if (payload.Data != null && payload.Data.TryGetValue("payout_id", out var payoutIdObj))
            {
                var payoutId = payoutIdObj?.ToString();
                if (!string.IsNullOrEmpty(payoutId))
                {
                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.GatewayTransactionId == payoutId);

                    if (payment != null)
                    {
                        webhook.RelatedPaymentId = payment.Id;
                        
                        // Update payment status based on webhook event
                        var previousStatus = payment.Status;
                        
                        switch (payload.Event?.ToLower())
                        {
                            case "payout.completed":
                                payment.Status = PaymentStatus.Completed;
                                _logger.LogInformation("Dots.dev payout {PayoutId} completed", payoutId);
                                break;
                                
                            case "payout.processing":
                                payment.Status = PaymentStatus.Processing;
                                break;
                                
                            case "payout.failed":
                                payment.Status = PaymentStatus.Failed;
                                // Store failure reason in metadata
                                var failureMetadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                                if (payload.Data.TryGetValue("error_message", out var errorMsg))
                                {
                                    failureMetadata["failureReason"] = errorMsg.ToString() ?? "Payout failed";
                                }
                                payment.Metadata = JsonSerializer.Serialize(failureMetadata);
                                _logger.LogWarning("Dots.dev payout {PayoutId} failed", payoutId);
                                break;
                        }

                        // Store webhook event in metadata
                        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                        metadata[$"webhook_{payload.Event}"] = DateTime.UtcNow.ToString("O");
                        payment.Metadata = JsonSerializer.Serialize(metadata);

                        // Create payment attempt record if status changed
                        if (previousStatus != payment.Status)
                        {
                            var attempt = new PaymentAttempt
                            {
                                PaymentId = payment.Id,
                                Status = payment.Status switch
                                {
                                    PaymentStatus.Completed => PaymentAttemptStatus.Succeeded,
                                    PaymentStatus.Failed => PaymentAttemptStatus.Failed,
                                    PaymentStatus.Processing => PaymentAttemptStatus.Processing,
                                    _ => PaymentAttemptStatus.Pending
                                },
                                GatewayResponse = JsonSerializer.Serialize(payload.Data),
                                AttemptNumber = 1,
                                Amount = payment.Amount,
                                Currency = payment.Currency,
                                ProcessedAt = DateTime.UtcNow,
                                ProcessingTimeMs = 0
                            };
                            
                            _context.PaymentAttempts.Add(attempt);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Payment not found for Dots.dev payout {PayoutId}", payoutId);
                    }
                }
            }

            // Mark webhook as processed
            webhook.ProcessedAt = DateTime.UtcNow;
            webhook.Status = WebhookStatus.Processed;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Dots.dev webhook processed successfully");
            return Ok(new { status = "success", received = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Dots.dev webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Handle MoonPay webhook notifications
    /// </summary>
    /// <returns>Webhook acknowledgment</returns>
    [HttpPost("moonpay")]
    public async Task<IActionResult> HandleMoonPayWebhook()
    {
        try
        {
            // Read raw body
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // Get MoonPay signature header
            var signature = Request.Headers["X-MoonPay-Signature"].ToString();
            
            _logger.LogInformation("Received MoonPay webhook, signature present: {HasSignature}", !string.IsNullOrEmpty(signature));

            // Parse webhook payload
            var payload = JsonSerializer.Deserialize<MoonPayWebhookPayload>(rawBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null)
            {
                _logger.LogWarning("Failed to parse MoonPay webhook payload");
                return BadRequest(new { error = "Invalid payload" });
            }

            // Store webhook record
            var webhook = new PaymentWebhook
            {
                GatewayType = PaymentGatewayType.MoonPay,
                EventType = payload.Type ?? "unknown",
                ExternalEventId = payload.EventId,
                Payload = rawBody,
                Status = WebhookStatus.Pending,
                ProcessedAt = null,
                Signature = signature,
                SourceIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Headers = JsonSerializer.Serialize(Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()))
            };

            _context.PaymentWebhooks.Add(webhook);
            await _context.SaveChangesAsync();

            // Process webhook with MoonPay service
            var moonPayService = HttpContext.RequestServices.GetService<IMoonPayService>();
            if (moonPayService == null)
            {
                _logger.LogError("MoonPay service not found");
                webhook.Status = WebhookStatus.Failed;
                webhook.ErrorMessage = "MoonPay service not configured";
                await _context.SaveChangesAsync();
                return StatusCode(500, new { error = "Service configuration error" });
            }

            var processed = await moonPayService.ProcessWebhookAsync(payload, signature);

            if (!processed)
            {
                webhook.Status = WebhookStatus.Failed;
                webhook.ErrorMessage = "Signature verification failed";
                await _context.SaveChangesAsync();
                
                _logger.LogWarning("MoonPay webhook processing failed");
                return Unauthorized(new { error = "Invalid signature" });
            }

            // Update payment based on webhook event
            if (payload.Data != null && !string.IsNullOrEmpty(payload.Data.ExternalTransactionId))
            {
                var paymentId = Guid.Parse(payload.Data.ExternalTransactionId);
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.Id == paymentId || p.GatewayTransactionId == payload.Data.Id);

                if (payment != null)
                {
                    webhook.RelatedPaymentId = payment.Id;
                    
                    // Update payment status based on webhook event
                    var previousStatus = payment.Status;
                    
                    switch (payload.Type?.ToLower())
                    {
                        case "transaction_created":
                            // Transaction just created, keep as pending
                            break;
                            
                        case "transaction_updated":
                            // Check the actual status from the data
                            if (payload.Data.Status?.ToLower() == "completed")
                            {
                                payment.Status = PaymentStatus.Completed;
                                _logger.LogInformation("MoonPay transaction {TransactionId} completed", payload.Data.Id);
                            }
                            else if (payload.Data.Status?.ToLower() == "failed")
                            {
                                payment.Status = PaymentStatus.Failed;
                                _logger.LogWarning("MoonPay transaction {TransactionId} failed", payload.Data.Id);
                            }
                            else if (payload.Data.Status?.ToLower() == "pending")
                            {
                                payment.Status = PaymentStatus.Pending;
                            }
                            break;
                            
                        case "transaction_failed":
                            payment.Status = PaymentStatus.Failed;
                            // Store failure reason in metadata
                            var failureMetadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                            failureMetadata["failureReason"] = payload.Data.FailureReason ?? "Transaction failed";
                            payment.Metadata = JsonSerializer.Serialize(failureMetadata);
                            _logger.LogWarning("MoonPay transaction {TransactionId} failed: {Reason}", 
                                payload.Data.Id, payload.Data.FailureReason);
                            break;
                    }

                    // Store webhook event and additional data in metadata
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                    metadata[$"webhook_{payload.Type}"] = DateTime.UtcNow.ToString("O");
                    
                    if (!string.IsNullOrEmpty(payload.Data.CryptoTransactionId))
                    {
                        metadata["cryptoTransactionId"] = payload.Data.CryptoTransactionId;
                    }
                    
                    if (payload.Data.CryptoAmount.HasValue)
                    {
                        metadata["cryptoAmount"] = payload.Data.CryptoAmount.Value.ToString();
                    }
                    
                    if (payload.Data.FiatAmount.HasValue)
                    {
                        metadata["fiatAmount"] = payload.Data.FiatAmount.Value.ToString();
                    }
                    
                    payment.Metadata = JsonSerializer.Serialize(metadata);

                    // Create payment attempt record if status changed
                    if (previousStatus != payment.Status)
                    {
                        var attempt = new PaymentAttempt
                        {
                            PaymentId = payment.Id,
                            Status = payment.Status switch
                            {
                                PaymentStatus.Completed => PaymentAttemptStatus.Succeeded,
                                PaymentStatus.Failed => PaymentAttemptStatus.Failed,
                                PaymentStatus.Processing => PaymentAttemptStatus.Processing,
                                _ => PaymentAttemptStatus.Pending
                            },
                            GatewayResponse = JsonSerializer.Serialize(payload.Data),
                            AttemptNumber = 1,
                            Amount = payment.Amount,
                            Currency = payment.Currency,
                            ProcessedAt = DateTime.UtcNow,
                            ProcessingTimeMs = 0
                        };
                        
                        _context.PaymentAttempts.Add(attempt);
                    }
                }
                else
                {
                    _logger.LogWarning("Payment not found for MoonPay transaction {TransactionId}", payload.Data.Id);
                }
            }

            // Mark webhook as processed
            webhook.ProcessedAt = DateTime.UtcNow;
            webhook.Status = WebhookStatus.Processed;
            await _context.SaveChangesAsync();

            _logger.LogInformation("MoonPay webhook processed successfully");
            return Ok(new { status = "success", received = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoonPay webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Handle Coinbase webhook notifications
    /// </summary>
    /// <returns>Webhook acknowledgment</returns>
    [HttpPost("coinbase")]
    public async Task<IActionResult> HandleCoinbaseWebhook()
    {
        try
        {
            // Read raw body
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // Get Coinbase signature header
            var signature = Request.Headers["CB-SIGNATURE"].ToString();
            var timestamp = Request.Headers["CB-TIMESTAMP"].ToString();
            
            _logger.LogInformation("Received Coinbase webhook, signature present: {HasSignature}, timestamp: {Timestamp}", 
                !string.IsNullOrEmpty(signature), timestamp);

            // Parse webhook payload
            var payload = JsonSerializer.Deserialize<CoinbaseWebhookPayload>(rawBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (payload == null)
            {
                _logger.LogWarning("Failed to parse Coinbase webhook payload");
                return BadRequest(new { error = "Invalid payload" });
            }

            // Store webhook record
            var webhook = new PaymentWebhook
            {
                GatewayType = PaymentGatewayType.Coinbase,
                EventType = payload.EventType ?? "unknown",
                ExternalEventId = payload.Id,
                Payload = rawBody,
                Status = WebhookStatus.Pending,
                ProcessedAt = null,
                Signature = signature,
                SourceIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Headers = JsonSerializer.Serialize(Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()))
            };

            _context.PaymentWebhooks.Add(webhook);
            await _context.SaveChangesAsync();

            // Process webhook based on event type
            if (payload.Data != null)
            {
                Payment? payment = null;
                
                // Find payment based on event type
                if (payload.EventType?.StartsWith("orders.") == true && payload.Data.TryGetValue("client_order_id", out var clientOrderId))
                {
                    var paymentId = Guid.Parse(clientOrderId.ToString() ?? "");
                    payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
                }
                else if (payload.Data.TryGetValue("order_id", out var orderId))
                {
                    payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.GatewayTransactionId == orderId.ToString());
                }

                if (payment != null)
                {
                    webhook.RelatedPaymentId = payment.Id;
                    
                    // Update payment status based on webhook event
                    var previousStatus = payment.Status;
                    
                    switch (payload.EventType?.ToLower())
                    {
                        case "orders.filled":
                            payment.Status = PaymentStatus.Completed;
                            
                            // Update filled details in metadata
                            var filledMetadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                            
                            if (payload.Data.TryGetValue("filled_size", out var filledSize))
                                filledMetadata["filledSize"] = filledSize.ToString();
                            
                            if (payload.Data.TryGetValue("average_filled_price", out var avgPrice))
                                filledMetadata["averageFilledPrice"] = avgPrice.ToString();
                            
                            if (payload.Data.TryGetValue("total_fees", out var fees))
                                filledMetadata["totalFees"] = fees.ToString();
                            
                            payment.Metadata = JsonSerializer.Serialize(filledMetadata);
                            _logger.LogInformation("Coinbase order filled. PaymentId: {PaymentId}", payment.Id);
                            break;
                            
                        case "orders.cancelled":
                            payment.Status = PaymentStatus.Cancelled;
                            _logger.LogInformation("Coinbase order cancelled. PaymentId: {PaymentId}", payment.Id);
                            break;
                            
                        case "orders.updated":
                            // Check order status in data
                            if (payload.Data.TryGetValue("status", out var status))
                            {
                                var statusStr = status.ToString()?.ToLower();
                                payment.Status = statusStr switch
                                {
                                    "filled" => PaymentStatus.Completed,
                                    "cancelled" => PaymentStatus.Cancelled,
                                    "failed" => PaymentStatus.Failed,
                                    "pending" => PaymentStatus.Pending,
                                    _ => PaymentStatus.Processing
                                };
                            }
                            break;
                            
                        case "orders.failed":
                            payment.Status = PaymentStatus.Failed;
                            
                            // Store failure reason in metadata
                            var failureMetadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                            
                            if (payload.Data.TryGetValue("reject_reason", out var rejectReason))
                                failureMetadata["failureReason"] = rejectReason.ToString() ?? "Order failed";
                            
                            payment.Metadata = JsonSerializer.Serialize(failureMetadata);
                            _logger.LogWarning("Coinbase order failed. PaymentId: {PaymentId}", payment.Id);
                            break;
                    }

                    // Store webhook event in metadata
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                    metadata[$"webhook_{payload.EventType}"] = DateTime.UtcNow.ToString("O");
                    payment.Metadata = JsonSerializer.Serialize(metadata);

                    // Create payment attempt record if status changed
                    if (previousStatus != payment.Status)
                    {
                        var attempt = new PaymentAttempt
                        {
                            PaymentId = payment.Id,
                            Status = payment.Status switch
                            {
                                PaymentStatus.Completed => PaymentAttemptStatus.Succeeded,
                                PaymentStatus.Failed => PaymentAttemptStatus.Failed,
                                PaymentStatus.Cancelled => PaymentAttemptStatus.Cancelled,
                                PaymentStatus.Processing => PaymentAttemptStatus.Processing,
                                _ => PaymentAttemptStatus.Pending
                            },
                            GatewayResponse = JsonSerializer.Serialize(payload.Data),
                            AttemptNumber = 1,
                            Amount = payment.Amount,
                            Currency = payment.Currency,
                            ProcessedAt = DateTime.UtcNow,
                            ProcessingTimeMs = 0
                        };
                        
                        _context.PaymentAttempts.Add(attempt);
                    }
                }
                else
                {
                    _logger.LogWarning("Payment not found for Coinbase webhook event {EventType}", payload.EventType);
                }
            }

            // Mark webhook as processed
            webhook.ProcessedAt = DateTime.UtcNow;
            webhook.Status = WebhookStatus.Processed;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Coinbase webhook processed successfully");
            return Ok(new { status = "success", received = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Coinbase webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Test webhook endpoint for development
    /// </summary>
    /// <returns>Test response</returns>
    [HttpPost("test")]
    [Authorize] // Require auth for test endpoint
    public IActionResult TestWebhook([FromBody] object payload)
    {
        _logger.LogInformation("Test webhook received: {Payload}", JsonSerializer.Serialize(payload));
        return Ok(new 
        { 
            message = "Test webhook received",
            timestamp = DateTime.UtcNow,
            headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        });
    }

    /// <summary>
    /// Get webhook status for monitoring
    /// </summary>
    /// <param name="paymentId">Payment ID to check webhooks for</param>
    /// <returns>Webhook status information</returns>
    [HttpGet("status/{paymentId}")]
    [Authorize]
    public async Task<IActionResult> GetWebhookStatus(Guid paymentId)
    {
        var webhooks = await _context.PaymentWebhooks
            .Where(w => w.RelatedPaymentId == paymentId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new
            {
                w.Id,
                w.EventType,
                Provider = w.GatewayType.ToString(),
                w.Status,
                ReceivedAt = w.CreatedAt,
                w.ProcessedAt,
                w.ErrorMessage
            })
            .ToListAsync();

        return Ok(new
        {
            paymentId,
            webhookCount = webhooks.Count,
            webhooks
        });
    }
}
