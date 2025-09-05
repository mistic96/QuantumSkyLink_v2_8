using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using QuantumLedger.Hub.Attributes;
using QuantumLedger.Hub.Services;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Cryptography.Models;

namespace QuantumLedger.Hub.Endpoints
{
    /// <summary>
    /// Substitution key management endpoints for the Delegation Key System.
    /// Provides user-controlled key generation, validation, and revocation.
    /// </summary>
    public static class SubstitutionKeyEndpoints
    {
        /// <summary>
        /// Maps all substitution key-related endpoints to the application.
        /// </summary>
        public static void MapSubstitutionKeyEndpoints(this IEndpointRouteBuilder app)
        {
            var substitutionGroup = app.MapGroup("/api/substitution-keys")
                .WithTags("Substitution Keys")
                .WithOpenApi();

            // Generate substitution key pair
            substitutionGroup.MapPost("/generate", GenerateSubstitutionKeyAsync)
                .WithName("GenerateSubstitutionKey")
                .WithSummary("Generate a new substitution key pair")
                .WithDescription("Creates a new substitution key pair for user-controlled delegation")
                .Produces<GenerateSubstitutionKeyResponse>(201)
                .Produces<SubstitutionKeyErrorResponse>(400)
                .Produces<SubstitutionKeyErrorResponse>(500);

            // Validate substitution key
            substitutionGroup.MapPost("/validate", ValidateSubstitutionKeyAsync)
                .WithName("ValidateSubstitutionKey")
                .WithSummary("Validate a substitution key")
                .WithDescription("Validates that a substitution key is active and not expired")
                .Produces<ValidateSubstitutionKeyResponse>(200)
                .Produces<SubstitutionKeyErrorResponse>(400)
                .Produces<SubstitutionKeyErrorResponse>(404)
                .Produces<SubstitutionKeyErrorResponse>(500);

            // Get substitution key info
            substitutionGroup.MapGet("/{substitutionKeyId}", GetSubstitutionKeyInfoAsync)
                .WithName("GetSubstitutionKeyInfo")
                .WithSummary("Get substitution key information")
                .WithDescription("Retrieves public information about a substitution key")
                .Produces<SubstitutionKeyInfoResponse>(200)
                .Produces<SubstitutionKeyErrorResponse>(404)
                .Produces<SubstitutionKeyErrorResponse>(500);

            // Revoke substitution key
            substitutionGroup.MapPost("/{substitutionKeyId}/revoke", RevokeSubstitutionKeyAsync)
                .WithName("RevokeSubstitutionKey")
                .WithSummary("Revoke a substitution key")
                .WithDescription("Revokes a substitution key, making it invalid for future use")
                .RequireAuthorization()
                .Produces<RevokeSubstitutionKeyResponse>(200)
                .Produces<SubstitutionKeyErrorResponse>(400)
                .Produces<SubstitutionKeyErrorResponse>(404)
                .Produces<SubstitutionKeyErrorResponse>(500);

            // List substitution keys for account
            substitutionGroup.MapGet("/account/{address}", ListAccountSubstitutionKeysAsync)
                .WithName("ListAccountSubstitutionKeys")
                .WithSummary("List substitution keys for an account")
                .WithDescription("Retrieves all substitution keys associated with an account address")
                .RequireAuthorization()
                .Produces<ListSubstitutionKeysResponse>(200)
                .Produces<SubstitutionKeyErrorResponse>(404)
                .Produces<SubstitutionKeyErrorResponse>(500);

            // Rotate substitution key
            substitutionGroup.MapPost("/{address}/rotate", RotateSubstitutionKeyAsync)
                .WithName("RotateSubstitutionKey")
                .WithSummary("Rotate a substitution key")
                .WithDescription("Creates a new substitution key and marks the old one as deprecated")
                .RequireAuthorization()
                .Produces<RotateSubstitutionKeyResponse>(200)
                .Produces<SubstitutionKeyErrorResponse>(400)
                .Produces<SubstitutionKeyErrorResponse>(404)
                .Produces<SubstitutionKeyErrorResponse>(500);

            // Verify substitution key signature
            substitutionGroup.MapPost("/verify-signature", VerifySubstitutionKeySignatureAsync)
                .WithName("VerifySubstitutionKeySignature")
                .WithSummary("Verify a signature made with a substitution key")
                .WithDescription("Verifies a digital signature using a substitution key")
                .Produces<SubstitutionKeySignatureVerificationResponse>(200)
                .Produces<SubstitutionKeyErrorResponse>(400)
                .Produces<SubstitutionKeyErrorResponse>(404)
                .Produces<SubstitutionKeyErrorResponse>(500);

            // Get substitution key usage statistics
            substitutionGroup.MapGet("/{substitutionKeyId}/stats", GetSubstitutionKeyStatsAsync)
                .WithName("GetSubstitutionKeyStats")
                .WithSummary("Get substitution key usage statistics")
                .WithDescription("Retrieves usage statistics and metrics for a substitution key")
                .RequireAuthorization()
                .Produces<SubstitutionKeyStatsResponse>(200)
                .Produces<SubstitutionKeyErrorResponse>(404)
                .Produces<SubstitutionKeyErrorResponse>(500);
        }

        /// <summary>
        /// Generates a new substitution key pair for user-controlled delegation.
        /// </summary>
        private static async Task<IResult> GenerateSubstitutionKeyAsync(
            GenerateSubstitutionKeyRequest request,
            ISubstitutionKeyService substitutionKeyService,
            IAuditLoggingService auditLoggingService,
            ILogger<string> logger)
        {
            try
            {
                logger.LogInformation("Generating substitution key for address {Address}", request.Address);

                var substitutionKey = await substitutionKeyService.GenerateSubstitutionKeyAsync(
                    request.Address, 
                    request.ExpiresAt);

                // Log successful substitution key generation for audit trail
                await auditLoggingService.LogSubstitutionKeyEventAsync(
                    SubstitutionKeyEventType.Generated,
                    substitutionKey.SubstitutionKeyId,
                    request.Address,
                    new { 
                        Algorithm = "EC256", 
                        ExpirationDate = substitutionKey.ExpiresAt,
                        RequestedBy = request.Address 
                    }
                );

                logger.LogInformation("Successfully generated substitution key {SubstitutionKeyId} for address {Address}",
                    substitutionKey.SubstitutionKeyId, request.Address);

                return Results.Created($"/api/substitution-keys/{substitutionKey.SubstitutionKeyId}", 
                    new GenerateSubstitutionKeyResponse
                    {
                        SubstitutionKeyId = substitutionKey.SubstitutionKeyId,
                        Address = request.Address,
                        PublicKey = substitutionKey.GetPublicKeyBase64(),
                        PrivateKey = substitutionKey.GetPrivateKeyBase64(), // Given to user for delegation
                        Algorithm = "EC256", // Default algorithm for substitution keys
                        CreatedAt = substitutionKey.CreatedAt,
                        ExpiresAt = substitutionKey.ExpiresAt,
                        Status = "Active",
                        SecurityNotice = "Store the private key securely. QuantumLedger does not retain private keys."
                    });
            }
            catch (ArgumentException ex)
            {
                // Log invalid request for security monitoring
                await auditLoggingService.LogSecurityEventAsync(
                    SecurityEventType.SuspiciousActivity,
                    $"Invalid substitution key generation request for address {request.Address}: {ex.Message}",
                    new { Address = request.Address, Error = ex.Message, RequestType = "GenerateSubstitutionKey" }
                );

                logger.LogWarning("Invalid substitution key generation request: {Message}", ex.Message);
                return Results.BadRequest(new SubstitutionKeyErrorResponse
                {
                    Error = "invalid_request",
                    ErrorDescription = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                // Log system error for security monitoring
                await auditLoggingService.LogSecurityEventAsync(
                    SecurityEventType.SystemAccess,
                    $"Failed substitution key generation for address {request.Address}: {ex.Message}",
                    new { Address = request.Address, Error = ex.Message, RequestType = "GenerateSubstitutionKey" }
                );

                logger.LogError(ex, "Error generating substitution key for address {Address}", request.Address);
                return Results.Problem(
                    title: "Substitution Key Generation Error",
                    detail: "An error occurred while generating the substitution key",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Validates that a substitution key is active and not expired.
        /// </summary>
        private static async Task<IResult> ValidateSubstitutionKeyAsync(
            ValidateSubstitutionKeyRequest request,
            ISubstitutionKeyService substitutionKeyService,
            IAuditLoggingService auditLoggingService,
            ILogger<string> logger)
        {
            try
            {
                logger.LogDebug("Validating substitution key {SubstitutionKeyId}", request.SubstitutionKeyId);

                var isValid = await substitutionKeyService.IsSubstitutionKeyActiveAsync(request.SubstitutionKeyId);

                // Log substitution key validation for audit trail
                await auditLoggingService.LogSubstitutionKeyEventAsync(
                    SubstitutionKeyEventType.Validated,
                    request.SubstitutionKeyId,
                    "unknown", // Address not available in validation request
                    new { 
                        IsValid = isValid,
                        ValidationResult = isValid ? "Valid" : "Invalid/Expired"
                    }
                );

                var response = new ValidateSubstitutionKeyResponse
                {
                    SubstitutionKeyId = request.SubstitutionKeyId,
                    IsValid = isValid,
                    ValidatedAt = DateTime.UtcNow,
                    Message = isValid ? "Substitution key is valid and active" : "Substitution key is invalid or expired"
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating substitution key {SubstitutionKeyId}", request.SubstitutionKeyId);
                return Results.Problem(
                    title: "Substitution Key Validation Error",
                    detail: "An error occurred while validating the substitution key",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Gets public information about a substitution key.
        /// </summary>
        private static async Task<IResult> GetSubstitutionKeyInfoAsync(
            string substitutionKeyId,
            ISubstitutionKeyService substitutionKeyService,
            ILogger<string> logger)
        {
            try
            {
                logger.LogDebug("Retrieving info for substitution key {SubstitutionKeyId}", substitutionKeyId);

                var publicKey = await substitutionKeyService.GetSubstitutionPublicKeyAsync(substitutionKeyId);
                var linkedAddress = await substitutionKeyService.GetLinkedAddressAsync(substitutionKeyId);
                var isActive = await substitutionKeyService.IsSubstitutionKeyActiveAsync(substitutionKeyId);
                var stats = await substitutionKeyService.GetSubstitutionKeyStatsAsync(substitutionKeyId);

                if (publicKey == null || linkedAddress == null)
                {
                    return Results.NotFound(new SubstitutionKeyErrorResponse
                    {
                        Error = "substitution_key_not_found",
                        ErrorDescription = $"Substitution key {substitutionKeyId} not found",
                        Timestamp = DateTime.UtcNow
                    });
                }

                var response = new SubstitutionKeyInfoResponse
                {
                    SubstitutionKeyId = substitutionKeyId,
                    Address = linkedAddress,
                    PublicKey = Convert.ToBase64String(publicKey),
                    Algorithm = "EC256",
                    CreatedAt = stats.ContainsKey("CreatedAt") ? (DateTime)stats["CreatedAt"] : DateTime.UtcNow,
                    ExpiresAt = stats.ContainsKey("ExpiresAt") ? (DateTime?)stats["ExpiresAt"] : null,
                    Status = isActive ? "Active" : "Inactive",
                    UsageCount = stats.ContainsKey("UsageCount") ? (int)stats["UsageCount"] : 0,
                    LastUsed = stats.ContainsKey("LastUsed") ? (DateTime?)stats["LastUsed"] : null,
                    IsExpired = !isActive,
                    RemainingDays = stats.ContainsKey("RemainingDays") ? (int?)stats["RemainingDays"] : null
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving info for substitution key {SubstitutionKeyId}", substitutionKeyId);
                return Results.Problem(
                    title: "Substitution Key Info Error",
                    detail: "An error occurred while retrieving substitution key information",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Revokes a substitution key, making it invalid for future use.
        /// </summary>
        [RequireSignature]
        private static async Task<IResult> RevokeSubstitutionKeyAsync(
            string substitutionKeyId,
            RevokeSubstitutionKeyRequest request,
            ISubstitutionKeyService substitutionKeyService,
            ILogger<string> logger)
        {
            try
            {
                logger.LogInformation("Revoking substitution key {SubstitutionKeyId} for reason: {Reason}",
                    substitutionKeyId, request.Reason);

                var success = await substitutionKeyService.RevokeSubstitutionKeyAsync(substitutionKeyId);

                if (!success)
                {
                    return Results.NotFound(new SubstitutionKeyErrorResponse
                    {
                        Error = "substitution_key_not_found",
                        ErrorDescription = $"Substitution key {substitutionKeyId} not found or already revoked",
                        Timestamp = DateTime.UtcNow
                    });
                }

                var response = new RevokeSubstitutionKeyResponse
                {
                    SubstitutionKeyId = substitutionKeyId,
                    RevokedAt = DateTime.UtcNow,
                    Reason = request.Reason ?? "User requested revocation",
                    Status = "Revoked",
                    Message = "Substitution key has been successfully revoked"
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error revoking substitution key {SubstitutionKeyId}", substitutionKeyId);
                return Results.Problem(
                    title: "Substitution Key Revocation Error",
                    detail: "An error occurred while revoking the substitution key",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Lists all substitution keys associated with an account address.
        /// </summary>
        [RequireSignature]
        private static async Task<IResult> ListAccountSubstitutionKeysAsync(
            string address,
            ISubstitutionKeyService substitutionKeyService,
            ILogger<string> logger,
            int page = 1,
            int pageSize = 20,
            string? status = null)
        {
            try
            {
                logger.LogDebug("Listing substitution keys for address {Address}", address);

                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var criteria = new SubstitutionKeyCriteria
                {
                    Address = address,
                    IncludeExpired = status == "all",
                    IncludeRevoked = status == "all"
                };

                var keys = await substitutionKeyService.GetSubstitutionKeysAsync(address, criteria);

                var keysList = keys.ToList();
                var totalCount = keysList.Count;
                var paginatedKeys = keysList.Skip((page - 1) * pageSize).Take(pageSize);

                var response = new ListSubstitutionKeysResponse
                {
                    Address = address,
                    Keys = paginatedKeys.Select(k => new SubstitutionKeyInfoResponse
                    {
                        SubstitutionKeyId = k.SubstitutionKeyId,
                        Address = k.LinkedAddress,
                        PublicKey = k.GetPublicKeyBase64(),
                        Algorithm = "EC256",
                        CreatedAt = k.CreatedAt,
                        ExpiresAt = k.ExpiresAt,
                        Status = k.IsActive ? "Active" : "Inactive",
                        UsageCount = 0, // Would need to get from stats
                        LastUsed = null, // Would need to get from stats
                        IsExpired = k.ExpiresAt.HasValue && k.ExpiresAt.Value <= DateTime.UtcNow,
                        RemainingDays = k.ExpiresAt.HasValue ? 
                            Math.Max(0, (int)(k.ExpiresAt.Value - DateTime.UtcNow).TotalDays) : null
                    }).ToList(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error listing substitution keys for address {Address}", address);
                return Results.Problem(
                    title: "Substitution Key Listing Error",
                    detail: "An error occurred while listing substitution keys",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Creates a new substitution key and marks the old one as deprecated.
        /// </summary>
        [RequireSignature]
        private static async Task<IResult> RotateSubstitutionKeyAsync(
            string address,
            RotateSubstitutionKeyRequest request,
            ISubstitutionKeyService substitutionKeyService,
            ILogger<string> logger)
        {
            try
            {
                logger.LogInformation("Rotating substitution key for address {Address}", address);

                var newKey = await substitutionKeyService.RotateSubstitutionKeyAsync(address);

                var response = new RotateSubstitutionKeyResponse
                {
                    OldSubstitutionKeyId = "deprecated", // Would need to track old key ID
                    NewSubstitutionKeyId = newKey.SubstitutionKeyId,
                    Address = newKey.LinkedAddress,
                    NewPublicKey = newKey.GetPublicKeyBase64(),
                    NewPrivateKey = newKey.GetPrivateKeyBase64(), // Given to user
                    Algorithm = "EC256",
                    RotatedAt = DateTime.UtcNow,
                    NewExpiresAt = newKey.ExpiresAt,
                    OldKeyStatus = "Deprecated",
                    SecurityNotice = "Store the new private key securely. The old key is now deprecated."
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error rotating substitution key for address {Address}", address);
                return Results.Problem(
                    title: "Substitution Key Rotation Error",
                    detail: "An error occurred while rotating the substitution key",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Verifies a digital signature using a substitution key.
        /// </summary>
        private static async Task<IResult> VerifySubstitutionKeySignatureAsync(
            VerifySubstitutionKeySignatureRequest request,
            ISubstitutionKeyService substitutionKeyService,
            IAuditLoggingService auditLoggingService,
            ILogger<string> logger)
        {
            try
            {
                logger.LogDebug("Verifying signature for substitution key {SubstitutionKeyId}", 
                    request.SubstitutionKeyId);

                var messageBytes = System.Text.Encoding.UTF8.GetBytes(request.Message);
                var verificationResult = await substitutionKeyService.VerifySubstitutionKeyRequestAsync(
                    messageBytes,
                    request.Signature,
                    request.SubstitutionKeyId,
                    null);

                // Log signature verification for audit trail
                var eventType = verificationResult.Success && verificationResult.SignatureValid 
                    ? SubstitutionKeyEventType.SignatureVerified 
                    : SubstitutionKeyEventType.SignatureRejected;

                await auditLoggingService.LogSubstitutionKeyEventAsync(
                    eventType,
                    request.SubstitutionKeyId,
                    verificationResult.AuthenticatedAddress ?? "unknown",
                    new { 
                        MessageLength = request.Message.Length,
                        SignatureLength = request.Signature.Length,
                        VerificationSuccess = verificationResult.Success,
                        SignatureValid = verificationResult.SignatureValid,
                        ErrorMessage = verificationResult.ErrorMessage
                    }
                );

                var response = new SubstitutionKeySignatureVerificationResponse
                {
                    IsValid = verificationResult.Success && verificationResult.SignatureValid,
                    SubstitutionKeyId = request.SubstitutionKeyId,
                    Algorithm = "EC256",
                    VerifiedAt = DateTime.UtcNow,
                    Message = verificationResult.Success ? 
                        "Signature verification successful" : 
                        "Signature verification failed",
                    ErrorCode = verificationResult.Success ? null : "verification_failed",
                    ErrorMessage = verificationResult.ErrorMessage,
                    AuthenticatedAddress = verificationResult.AuthenticatedAddress,
                    VerificationTimeMs = 25 // Mock value
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error verifying signature for substitution key {SubstitutionKeyId}", 
                    request.SubstitutionKeyId);
                return Results.Problem(
                    title: "Signature Verification Error",
                    detail: "An error occurred while verifying the signature",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Gets usage statistics for a substitution key.
        /// </summary>
        [RequireSignature]
        private static async Task<IResult> GetSubstitutionKeyStatsAsync(
            string substitutionKeyId,
            ISubstitutionKeyService substitutionKeyService,
            ILogger<string> logger,
            int days = 30)
        {
            try
            {
                logger.LogDebug("Retrieving stats for substitution key {SubstitutionKeyId} for {Days} days", 
                    substitutionKeyId, days);

                var stats = await substitutionKeyService.GetSubstitutionKeyStatsAsync(substitutionKeyId);

                if (stats == null || stats.Count == 0)
                {
                    return Results.NotFound(new SubstitutionKeyErrorResponse
                    {
                        Error = "substitution_key_not_found",
                        ErrorDescription = $"Substitution key {substitutionKeyId} not found",
                        Timestamp = DateTime.UtcNow
                    });
                }

                var response = new SubstitutionKeyStatsResponse
                {
                    SubstitutionKeyId = substitutionKeyId,
                    PeriodDays = days,
                    TotalSignatures = stats.ContainsKey("TotalSignatures") ? (int)stats["TotalSignatures"] : 0,
                    TotalTransactions = stats.ContainsKey("TotalTransactions") ? (int)stats["TotalTransactions"] : 0,
                    AverageSignaturesPerDay = stats.ContainsKey("AverageSignaturesPerDay") ? (double)stats["AverageSignaturesPerDay"] : 0,
                    LastUsed = stats.ContainsKey("LastUsed") ? (DateTime?)stats["LastUsed"] : null,
                    MostActiveDay = stats.ContainsKey("MostActiveDay") ? (DateTime?)stats["MostActiveDay"] : null,
                    SecurityScore = stats.ContainsKey("SecurityScore") ? (double)stats["SecurityScore"] : 100.0,
                    UsagePattern = stats.ContainsKey("UsagePattern") ? (string)stats["UsagePattern"] : "Normal"
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving stats for substitution key {SubstitutionKeyId}", substitutionKeyId);
                return Results.Problem(
                    title: "Substitution Key Stats Error",
                    detail: "An error occurred while retrieving substitution key statistics",
                    statusCode: 500);
            }
        }
    }

    #region Request/Response Models

    public class GenerateSubstitutionKeyRequest
    {
        [Required]
        public string Address { get; set; } = string.Empty;
        
        public DateTime? ExpiresAt { get; set; }
    }

    public class GenerateSubstitutionKeyResponse
    {
        public string SubstitutionKeyId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty; // Given to user
        public string Algorithm { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SecurityNotice { get; set; } = string.Empty;
    }

    public class ValidateSubstitutionKeyRequest
    {
        [Required]
        public string SubstitutionKeyId { get; set; } = string.Empty;
    }

    public class ValidateSubstitutionKeyResponse
    {
        public string SubstitutionKeyId { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public DateTime ValidatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SubstitutionKeyInfoResponse
    {
        public string SubstitutionKeyId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string Algorithm { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool IsExpired { get; set; }
        public int? RemainingDays { get; set; }
    }

    public class RevokeSubstitutionKeyRequest
    {
        public string? Reason { get; set; }
    }

    public class RevokeSubstitutionKeyResponse
    {
        public string SubstitutionKeyId { get; set; } = string.Empty;
        public DateTime RevokedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ListSubstitutionKeysResponse
    {
        public string Address { get; set; } = string.Empty;
        public List<SubstitutionKeyInfoResponse> Keys { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class RotateSubstitutionKeyRequest
    {
        public DateTime? ExpiresAt { get; set; }
    }

    public class RotateSubstitutionKeyResponse
    {
        public string OldSubstitutionKeyId { get; set; } = string.Empty;
        public string NewSubstitutionKeyId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string NewPublicKey { get; set; } = string.Empty;
        public string NewPrivateKey { get; set; } = string.Empty; // Given to user
        public string Algorithm { get; set; } = string.Empty;
        public DateTime RotatedAt { get; set; }
        public DateTime? NewExpiresAt { get; set; }
        public string OldKeyStatus { get; set; } = string.Empty;
        public string SecurityNotice { get; set; } = string.Empty;
    }

    public class VerifySubstitutionKeySignatureRequest
    {
        [Required]
        public string SubstitutionKeyId { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string Signature { get; set; } = string.Empty;
        
        public string? Nonce { get; set; }
    }

    public class SubstitutionKeySignatureVerificationResponse
    {
        public bool IsValid { get; set; }
        public string SubstitutionKeyId { get; set; } = string.Empty;
        public string Algorithm { get; set; } = string.Empty;
        public DateTime VerifiedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? AuthenticatedAddress { get; set; }
        public double VerificationTimeMs { get; set; }
    }

    public class SubstitutionKeyStatsResponse
    {
        public string SubstitutionKeyId { get; set; } = string.Empty;
        public int PeriodDays { get; set; }
        public int TotalSignatures { get; set; }
        public int TotalTransactions { get; set; }
        public double AverageSignaturesPerDay { get; set; }
        public DateTime? LastUsed { get; set; }
        public DateTime? MostActiveDay { get; set; }
        public double SecurityScore { get; set; }
        public string UsagePattern { get; set; } = string.Empty;
    }

    public class SubstitutionKeyErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? ErrorCode { get; set; }
    }

    #endregion
}
