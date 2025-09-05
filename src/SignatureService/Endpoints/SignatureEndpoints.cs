using SignatureService.Models;
using SignatureService.Services;

namespace SignatureService.Endpoints;

/// <summary>
/// Core signature validation endpoints using Minimal APIs
/// Follows the same pattern as QuantumLedger's endpoint structure
/// </summary>
public static class SignatureEndpoints
{
    /// <summary>
    /// Maps all signature validation endpoints to the application
    /// </summary>
    public static void MapSignatureEndpoints(this IEndpointRouteBuilder app)
    {
        var signatureGroup = app.MapGroup("/api/v1/signatures")
            .WithTags("Signatures")
            .WithOpenApi();

        // Universal signature validation endpoint
        signatureGroup.MapPost("/validate", ValidateSignatureAsync)
            .WithName("ValidateSignature")
            .WithSummary("Validate universal signature with replay protection")
            .WithDescription("Validates signatures from any system (QuantumSkyLink v2, QuantumLedger, etc.) with comprehensive security checks")
            .Produces<SignatureValidationResult>(200)
            .Produces<ErrorResponse>(400)
            .Produces<ErrorResponse>(500);

        // Dual signature validation endpoint (for QuantumLedger)
        signatureGroup.MapPost("/validate-dual", ValidateDualSignatureAsync)
            .WithName("ValidateDualSignature")
            .WithSummary("Validate dual signature (classic + quantum)")
            .WithDescription("Validates QuantumLedger's dual signature system with both classic and quantum cryptography")
            .Produces<DualSignatureValidationResult>(200)
            .Produces<ErrorResponse>(400)
            .Produces<ErrorResponse>(500);

        // Transaction confirmation endpoint
        signatureGroup.MapPost("/confirm", ConfirmTransactionAsync)
            .WithName("ConfirmTransaction")
            .WithSummary("Confirm transaction signature")
            .WithDescription("Confirms a transaction signature after successful processing")
            .Produces<TransactionConfirmationResult>(200)
            .Produces<ErrorResponse>(400)
            .Produces<ErrorResponse>(500);

        // Nonce validation endpoint
        signatureGroup.MapPost("/validate-nonce", ValidateNonceAsync)
            .WithName("ValidateNonce")
            .WithSummary("Validate nonce and sequence number")
            .WithDescription("Validates nonce for replay protection without full signature verification")
            .Produces<NonceValidationResult>(200)
            .Produces<ErrorResponse>(400)
            .Produces<ErrorResponse>(500);

        // Health check endpoint
        signatureGroup.MapGet("/health", GetHealthAsync)
            .WithName("GetSignatureServiceHealth")
            .WithSummary("Get signature service health status")
            .WithDescription("Returns health status and performance metrics")
            .Produces<Dictionary<string, object>>(200)
            .Produces<ErrorResponse>(500);
    }

    /// <summary>
    /// Validates a universal signature request
    /// Target: ≤1 second response time
    /// </summary>
    private static async Task<IResult> ValidateSignatureAsync(
        UniversalSignatureValidationRequest request,
        SignatureValidationService validationService,
        ILogger<string> logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Enhanced input validation (same pattern as QuantumLedger)
            if (request == null)
            {
                logger.LogWarning("Received null signature validation request");
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_request",
                    ErrorDescription = "Request cannot be null",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (string.IsNullOrWhiteSpace(request.AccountId))
            {
                logger.LogWarning("Received signature validation request with empty account ID");
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_account_id",
                    ErrorDescription = "Account ID cannot be empty",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (string.IsNullOrWhiteSpace(request.Signature))
            {
                logger.LogWarning("Received signature validation request with empty signature for account {AccountId}", request.AccountId);
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_signature",
                    ErrorDescription = "Signature cannot be empty",
                    Timestamp = DateTime.UtcNow
                });
            }

            logger.LogInformation("Processing signature validation request for account {AccountId}, operation {Operation}", 
                request.AccountId, request.Operation);

            // Validate signature using QuantumLedger's cryptographic infrastructure
            var result = await validationService.ValidateSignatureAsync(request, cancellationToken);

            if (!result.IsValid)
            {
                logger.LogWarning("Signature validation failed for account {AccountId}: {Error}", 
                    request.AccountId, result.Error);
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "signature_validation_failed",
                    ErrorDescription = result.Error,
                    Timestamp = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        ["validation_id"] = result.ValidationId,
                        ["processing_time_ms"] = result.ProcessingTime.TotalMilliseconds
                    }
                });
            }

            logger.LogInformation("Signature validation successful for account {AccountId}, validation ID {ValidationId}, processing time {ProcessingTimeMs}ms", 
                request.AccountId, result.ValidationId, result.ProcessingTime.TotalMilliseconds);
            
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing signature validation request");
            return Results.Problem(
                title: "Signature Validation Error",
                detail: "An error occurred while validating the signature",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Validates a dual signature (classic + quantum)
    /// Target: ≤1 second response time
    /// </summary>
    private static async Task<IResult> ValidateDualSignatureAsync(
        DualSignatureValidationRequest request,
        SignatureValidationService validationService,
        ILogger<string> logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Enhanced input validation
            if (request == null)
            {
                logger.LogWarning("Received null dual signature validation request");
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_request",
                    ErrorDescription = "Request cannot be null",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (string.IsNullOrWhiteSpace(request.AccountId))
            {
                logger.LogWarning("Received dual signature validation request with empty account ID");
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_account_id",
                    ErrorDescription = "Account ID cannot be empty",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (request.Signature == null)
            {
                logger.LogWarning("Received dual signature validation request with null signature for account {AccountId}", request.AccountId);
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_signature",
                    ErrorDescription = "Dual signature cannot be null",
                    Timestamp = DateTime.UtcNow
                });
            }

            logger.LogInformation("Processing dual signature validation request for account {AccountId}", request.AccountId);

            // Validate both classic and quantum signatures using QuantumLedger's DualSignature model
            var result = await validationService.ValidateDualSignatureAsync(request, cancellationToken);

            if (!result.Success)
            {
                logger.LogWarning("Dual signature validation failed for account {AccountId}: {Error} - Classic: {ClassicValid}, Quantum: {QuantumValid}", 
                    request.AccountId, result.ErrorMessage, result.ClassicValid, result.QuantumValid);
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "dual_signature_validation_failed",
                    ErrorDescription = result.ErrorMessage ?? "Dual signature validation failed",
                    Timestamp = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        ["validation_id"] = result.ValidationId,
                        ["classic_valid"] = result.ClassicValid,
                        ["quantum_valid"] = result.QuantumValid,
                        ["processing_time_ms"] = result.ProcessingTime.TotalMilliseconds
                    }
                });
            }

            logger.LogInformation("Dual signature validation successful for account {AccountId}, validation ID {ValidationId} - Classic: {ClassicValid}, Quantum: {QuantumValid}, processing time {ProcessingTimeMs}ms", 
                request.AccountId, result.ValidationId, result.ClassicValid, result.QuantumValid, result.ProcessingTime.TotalMilliseconds);
            
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing dual signature validation request");
            return Results.Problem(
                title: "Dual Signature Validation Error",
                detail: "An error occurred while validating the dual signature",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Confirms a transaction signature
    /// </summary>
    private static async Task<IResult> ConfirmTransactionAsync(
        TransactionConfirmationRequest request,
        ILogger<string> logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Enhanced input validation
            if (request == null)
            {
                logger.LogWarning("Received null transaction confirmation request");
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_request",
                    ErrorDescription = "Request cannot be null",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (string.IsNullOrWhiteSpace(request.OriginalValidationId))
            {
                logger.LogWarning("Received transaction confirmation request with empty validation ID");
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_validation_id",
                    ErrorDescription = "Original validation ID cannot be empty",
                    Timestamp = DateTime.UtcNow
                });
            }

            logger.LogInformation("Processing transaction confirmation for validation ID {ValidationId}", request.OriginalValidationId);

            // For now, we'll implement a simple confirmation
            // In a full implementation, this would verify the transaction result signature
            var result = new TransactionConfirmationResult
            {
                Success = true,
                ConfirmationId = Guid.NewGuid().ToString(),
                ConfirmedAt = DateTime.UtcNow
            };

            logger.LogInformation("Transaction confirmation successful for validation ID {ValidationId}, confirmation ID {ConfirmationId}", 
                request.OriginalValidationId, result.ConfirmationId);
            
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing transaction confirmation request");
            return Results.Problem(
                title: "Transaction Confirmation Error",
                detail: "An error occurred while confirming the transaction",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Validates a nonce without full signature verification
    /// </summary>
    private static async Task<IResult> ValidateNonceAsync(
        NonceValidationRequest request,
        NonceTrackingService nonceTracker,
        ILogger<string> logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Enhanced input validation
            if (request == null)
            {
                logger.LogWarning("Received null nonce validation request");
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_request",
                    ErrorDescription = "Request cannot be null",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (string.IsNullOrWhiteSpace(request.AccountId))
            {
                logger.LogWarning("Received nonce validation request with empty account ID");
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_account_id",
                    ErrorDescription = "Account ID cannot be empty",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (string.IsNullOrWhiteSpace(request.Nonce))
            {
                logger.LogWarning("Received nonce validation request with empty nonce for account {AccountId}", request.AccountId);
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_nonce",
                    ErrorDescription = "Nonce cannot be empty",
                    Timestamp = DateTime.UtcNow
                });
            }

            logger.LogDebug("Processing nonce validation request for account {AccountId}, nonce {Nonce}", 
                request.AccountId, request.Nonce);

            // Validate nonce using the nonce tracking service
            var result = await nonceTracker.ValidateNonceAsync(
                request.AccountId, request.Nonce, request.SequenceNumber, request.Timestamp);

            if (!result.IsValid)
            {
                logger.LogWarning("Nonce validation failed for account {AccountId}: {Error}", 
                    request.AccountId, result.Error);
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "nonce_validation_failed",
                    ErrorDescription = result.Error,
                    Timestamp = DateTime.UtcNow
                });
            }

            logger.LogDebug("Nonce validation successful for account {AccountId}", request.AccountId);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing nonce validation request");
            return Results.Problem(
                title: "Nonce Validation Error",
                detail: "An error occurred while validating the nonce",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Gets health status and performance metrics
    /// </summary>
    private static async Task<IResult> GetHealthAsync(
        ILogger<string> logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var health = new Dictionary<string, object>
            {
                ["status"] = "healthy",
                ["service"] = "SignatureService",
                ["version"] = "1.0.0",
                ["timestamp"] = DateTime.UtcNow,
                ["uptime"] = Environment.TickCount64,
                ["performance_targets"] = new Dictionary<string, object>
                {
                    ["signature_validation"] = "≤1 second",
                    ["nonce_validation"] = "≤200ms",
                    ["dual_signature_validation"] = "≤1 second"
                },
                ["supported_algorithms"] = new[]
                {
                    "Dilithium",
                    "Falcon", 
                    "EC256"
                },
                ["features"] = new[]
                {
                    "Universal signature validation",
                    "Dual signature support",
                    "Replay attack prevention",
                    "Multi-algorithm support",
                    "Performance optimization"
                }
            };

            return Results.Ok(health);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting health status");
            return Results.Problem(
                title: "Health Check Error",
                detail: "An error occurred while checking service health",
                statusCode: 500);
        }
    }
}
