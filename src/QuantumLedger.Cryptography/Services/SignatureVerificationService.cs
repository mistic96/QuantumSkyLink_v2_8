using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Data;
using QuantumLedger.Models.Account;

namespace QuantumLedger.Cryptography.Services
{
    /// <summary>
    /// Service for verifying digital signatures with multi-algorithm support and replay protection.
    /// Implements high-performance signature verification with nonce validation.
    /// </summary>
    public class SignatureVerificationService
    {
        private readonly AccountsContext _accountsContext;
        private readonly ISignatureProvider _dilithiumProvider;
        private readonly ISignatureProvider _falconProvider;
        private readonly ISignatureProvider _ec256Provider;
        private readonly ILogger<SignatureVerificationService> _logger;

        // Valid algorithms for signature verification
        private static readonly string[] ValidAlgorithms = { "Dilithium", "Falcon", "EC256" };

        // Performance metrics
        private long _verificationAttempts = 0;
        private long _successfulVerifications = 0;
        private long _failedVerifications = 0;
        private long _replayAttacks = 0;
        private readonly DateTime _statsStartTime = DateTime.UtcNow;

        public SignatureVerificationService(
            AccountsContext accountsContext,
            IEnumerable<ISignatureProvider> signatureProviders,
            ILogger<SignatureVerificationService> logger)
        {
            _accountsContext = accountsContext ?? throw new ArgumentNullException(nameof(accountsContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize signature providers by algorithm
            var providers = signatureProviders.ToList();
            _dilithiumProvider = providers.FirstOrDefault(p => p.Algorithm == "Dilithium") 
                ?? throw new InvalidOperationException("Dilithium signature provider not found");
            _falconProvider = providers.FirstOrDefault(p => p.Algorithm == "Falcon") 
                ?? throw new InvalidOperationException("Falcon signature provider not found");
            _ec256Provider = providers.FirstOrDefault(p => p.Algorithm == "EC256") 
                ?? throw new InvalidOperationException("EC256 signature provider not found");

            _logger.LogInformation("Signature Verification Service initialized with multi-algorithm support");
        }

        /// <summary>
        /// Verifies a signed request with replay protection and multi-algorithm support.
        /// </summary>
        /// <param name="request">The signed request to verify.</param>
        /// <returns>The verification result.</returns>
        public async Task<SignatureVerificationResult> VerifySignedRequestAsync(SignedRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Interlocked.Increment(ref _verificationAttempts);

            try
            {
                _logger.LogDebug("Starting signature verification for account {AccountId} with algorithm {Algorithm}",
                    request.Signature.AccountId, request.Signature.Algorithm);

                // Step 1: Validate request structure
                var structureValidation = ValidateRequestStructure(request);
                if (!structureValidation.IsValid)
                {
                    Interlocked.Increment(ref _failedVerifications);
                    return structureValidation;
                }

                // Step 2: Check for replay attacks
                var replayCheck = await CheckReplayProtectionAsync(request);
                if (!replayCheck.IsValid)
                {
                    Interlocked.Increment(ref _replayAttacks);
                    Interlocked.Increment(ref _failedVerifications);
                    return replayCheck;
                }

                // Step 3: Retrieve public key from registry
                var publicKeyResult = await GetPublicKeyFromRegistryAsync(request.Signature.AccountId, request.Signature.Algorithm);
                if (!publicKeyResult.IsValid)
                {
                    Interlocked.Increment(ref _failedVerifications);
                    return new SignatureVerificationResult
                    {
                        IsValid = false,
                        ErrorCode = publicKeyResult.ErrorCode,
                        ErrorMessage = publicKeyResult.ErrorMessage,
                        VerifiedAt = DateTime.UtcNow
                    };
                }

                // Step 4: Verify signature
                var signatureResult = await VerifySignatureAsync(request, publicKeyResult.PublicKey!, publicKeyResult.Algorithm!);
                if (!signatureResult.IsValid)
                {
                    Interlocked.Increment(ref _failedVerifications);
                    return signatureResult;
                }

                // Step 5: Record nonce to prevent replay
                await RecordNonceAsync(request);

                // Step 6: Update public key usage statistics
                await UpdatePublicKeyUsageAsync(publicKeyResult.PublicKeyHash!);

                Interlocked.Increment(ref _successfulVerifications);

                _logger.LogInformation("Successfully verified signature for account {AccountId} with algorithm {Algorithm}",
                    request.Signature.AccountId, request.Signature.Algorithm);

                return new SignatureVerificationResult
                {
                    IsValid = true,
                    AccountId = request.Signature.AccountId,
                    Algorithm = request.Signature.Algorithm,
                    VerifiedAt = DateTime.UtcNow,
                    Message = "Signature verification successful"
                };
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failedVerifications);
                _logger.LogError(ex, "Signature verification failed for account {AccountId}",
                    request.Signature?.AccountId);

                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "VERIFICATION_ERROR",
                    ErrorMessage = "Internal verification error",
                    VerifiedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Validates the structure of a signed request.
        /// </summary>
        private SignatureVerificationResult ValidateRequestStructure(SignedRequest request)
        {
            if (request.Payload == null)
            {
                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "INVALID_PAYLOAD",
                    ErrorMessage = "Request payload is required",
                    VerifiedAt = DateTime.UtcNow
                };
            }

            if (request.Signature == null)
            {
                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "INVALID_SIGNATURE",
                    ErrorMessage = "Request signature is required",
                    VerifiedAt = DateTime.UtcNow
                };
            }

            if (request.Signature.AccountId == Guid.Empty)
            {
                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "INVALID_ACCOUNT_ID",
                    ErrorMessage = "Valid account ID is required",
                    VerifiedAt = DateTime.UtcNow
                };
            }

            if (string.IsNullOrWhiteSpace(request.Signature.Algorithm))
            {
                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "INVALID_ALGORITHM",
                    ErrorMessage = "Signature algorithm is required",
                    VerifiedAt = DateTime.UtcNow
                };
            }

            if (!ValidAlgorithms.Contains(request.Signature.Algorithm))
            {
                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "UNSUPPORTED_ALGORITHM",
                    ErrorMessage = $"Algorithm '{request.Signature.Algorithm}' is not supported",
                    VerifiedAt = DateTime.UtcNow
                };
            }

            if (string.IsNullOrWhiteSpace(request.Signature.SignatureValue))
            {
                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "INVALID_SIGNATURE_VALUE",
                    ErrorMessage = "Signature value is required",
                    VerifiedAt = DateTime.UtcNow
                };
            }

            if (string.IsNullOrWhiteSpace(request.Signature.Nonce))
            {
                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "INVALID_NONCE",
                    ErrorMessage = "Nonce is required for replay protection",
                    VerifiedAt = DateTime.UtcNow
                };
            }

            // Validate timestamp (must be within acceptable time window)
            var timeDiff = Math.Abs((DateTime.UtcNow - request.Signature.Timestamp).TotalMinutes);
            if (timeDiff > 60) // Max 60 minutes
            {
                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "TIMESTAMP_OUT_OF_RANGE",
                    ErrorMessage = "Request timestamp is too old (max 60 minutes)",
                    VerifiedAt = DateTime.UtcNow
                };
            }

            return new SignatureVerificationResult { IsValid = true };
        }

        /// <summary>
        /// Checks for replay attacks using nonce validation.
        /// </summary>
        private async Task<SignatureVerificationResult> CheckReplayProtectionAsync(SignedRequest request)
        {
            try
            {
                var nonceHash = GenerateNonceHash(request.Signature.Nonce);

                // Check if nonce has been used before
                var existingNonce = await _accountsContext.RequestNonces
                    .FirstOrDefaultAsync(n => n.NonceHash == nonceHash && n.AccountId == request.Signature.AccountId);

                if (existingNonce != null)
                {
                    _logger.LogWarning("Replay attack detected for account {AccountId} with nonce {NonceHash}",
                        request.Signature.AccountId, nonceHash);

                    return new SignatureVerificationResult
                    {
                        IsValid = false,
                        ErrorCode = "REPLAY_ATTACK",
                        ErrorMessage = "Nonce has already been used (replay attack detected)",
                        VerifiedAt = DateTime.UtcNow
                    };
                }

                return new SignatureVerificationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking replay protection for account {AccountId}",
                    request.Signature.AccountId);

                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "REPLAY_CHECK_ERROR",
                    ErrorMessage = "Error checking replay protection",
                    VerifiedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Retrieves the public key from the registry for signature verification.
        /// </summary>
        private async Task<PublicKeyResult> GetPublicKeyFromRegistryAsync(Guid accountId, string algorithm)
        {
            try
            {
                var publicKeyEntry = await _accountsContext.PublicKeyRegistry
                    .FirstOrDefaultAsync(p => p.AccountId == accountId && 
                                            p.Algorithm == algorithm && 
                                            p.Status == "Active");

                if (publicKeyEntry == null)
                {
                    _logger.LogWarning("Public key not found for account {AccountId} with algorithm {Algorithm}",
                        accountId, algorithm);

                    return new PublicKeyResult
                    {
                        IsValid = false,
                        ErrorCode = "PUBLIC_KEY_NOT_FOUND",
                        ErrorMessage = $"No active public key found for account {accountId} with algorithm {algorithm}"
                    };
                }

                return new PublicKeyResult
                {
                    IsValid = true,
                    PublicKey = publicKeyEntry.PublicKey,
                    Algorithm = publicKeyEntry.Algorithm,
                    PublicKeyHash = publicKeyEntry.PublicKeyHash
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public key for account {AccountId} with algorithm {Algorithm}",
                    accountId, algorithm);

                return new PublicKeyResult
                {
                    IsValid = false,
                    ErrorCode = "PUBLIC_KEY_RETRIEVAL_ERROR",
                    ErrorMessage = "Error retrieving public key from registry"
                };
            }
        }

        /// <summary>
        /// Verifies the digital signature using the appropriate algorithm provider.
        /// </summary>
        private async Task<SignatureVerificationResult> VerifySignatureAsync(SignedRequest request, string publicKey, string algorithm)
        {
            try
            {
                // Serialize payload for signature verification
                var payloadJson = JsonSerializer.Serialize(request.Payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
                var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

                // Decode signature from base64
                var signatureBytes = Convert.FromBase64String(request.Signature.SignatureValue);
                var publicKeyBytes = Convert.FromBase64String(publicKey);

                // Select appropriate signature provider
                ISignatureProvider provider = algorithm switch
                {
                    "Dilithium" => _dilithiumProvider,
                    "Falcon" => _falconProvider,
                    "EC256" => _ec256Provider,
                    _ => throw new NotSupportedException($"Algorithm '{algorithm}' is not supported")
                };

                // Verify signature
                var isValid = await provider.VerifyAsync(payloadBytes, signatureBytes, publicKeyBytes);

                if (!isValid)
                {
                    _logger.LogWarning("Signature verification failed for account {AccountId} with algorithm {Algorithm}",
                        request.Signature.AccountId, algorithm);

                    return new SignatureVerificationResult
                    {
                        IsValid = false,
                        ErrorCode = "SIGNATURE_INVALID",
                        ErrorMessage = "Digital signature verification failed",
                        VerifiedAt = DateTime.UtcNow
                    };
                }

                return new SignatureVerificationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying signature for account {AccountId} with algorithm {Algorithm}",
                    request.Signature.AccountId, algorithm);

                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "SIGNATURE_VERIFICATION_ERROR",
                    ErrorMessage = "Error during signature verification",
                    VerifiedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Records the nonce to prevent replay attacks.
        /// </summary>
        private async Task RecordNonceAsync(SignedRequest request)
        {
            try
            {
                var requestNonce = new RequestNonce
                {
                    NonceHash = GenerateNonceHash(request.Signature.Nonce),
                    AccountId = request.Signature.AccountId,
                    OriginalNonce = request.Signature.Nonce,
                    Timestamp = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15 minutes default expiration
                    RequestType = request.Payload?.GetType().Name
                };

                _accountsContext.RequestNonces.Add(requestNonce);
                await _accountsContext.SaveChangesAsync();

                _logger.LogDebug("Recorded nonce {NonceHash} for account {AccountId}",
                    requestNonce.NonceHash, request.Signature.AccountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording nonce for account {AccountId}",
                    request.Signature.AccountId);
                // Don't fail verification for nonce recording errors
            }
        }

        /// <summary>
        /// Updates public key usage statistics.
        /// </summary>
        private async Task UpdatePublicKeyUsageAsync(string publicKeyHash)
        {
            try
            {
                var publicKeyEntry = await _accountsContext.PublicKeyRegistry
                    .FirstOrDefaultAsync(p => p.PublicKeyHash == publicKeyHash);

                if (publicKeyEntry != null)
                {
                    publicKeyEntry.UsageCount++;
                    publicKeyEntry.LastUsed = DateTime.UtcNow;
                    await _accountsContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating public key usage for hash {PublicKeyHash}", publicKeyHash);
                // Don't fail verification for usage tracking errors
            }
        }

        /// <summary>
        /// Generates a SHA-256 hash of the nonce for storage and lookup.
        /// </summary>
        private static string GenerateNonceHash(string nonce)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(nonce));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Gets verification statistics for monitoring.
        /// </summary>
        public async Task<VerificationStats> GetVerificationStatsAsync()
        {
            await Task.CompletedTask; // Async for interface compliance

            var periodEnd = DateTime.UtcNow;
            var totalAttempts = _verificationAttempts;
            var successRate = totalAttempts > 0 ? (double)_successfulVerifications / totalAttempts * 100 : 0;

            return new VerificationStats
            {
                TotalAttempts = totalAttempts,
                SuccessfulVerifications = _successfulVerifications,
                FailedVerifications = _failedVerifications,
                ReplayAttacks = _replayAttacks,
                SuccessRate = successRate,
                PeriodStart = _statsStartTime,
                PeriodEnd = periodEnd,
                VerificationsPerSecond = totalAttempts / Math.Max(1, (periodEnd - _statsStartTime).TotalSeconds)
            };
        }

        /// <summary>
        /// Cleans up expired nonces to maintain database performance.
        /// </summary>
        public async Task CleanupExpiredNoncesAsync()
        {
            try
            {
                var expiredNonces = await _accountsContext.RequestNonces
                    .Where(n => n.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync();

                if (expiredNonces.Any())
                {
                    _accountsContext.RequestNonces.RemoveRange(expiredNonces);
                    await _accountsContext.SaveChangesAsync();

                    _logger.LogInformation("Cleaned up {Count} expired nonces", expiredNonces.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired nonces");
            }
        }
    }

    #region Request/Response Models

    /// <summary>
    /// Represents a signed request with payload and signature.
    /// </summary>
    public class SignedRequest
    {
        /// <summary>
        /// Gets or sets the request payload.
        /// </summary>
        public object Payload { get; set; } = null!;

        /// <summary>
        /// Gets or sets the digital signature.
        /// </summary>
        public RequestSignature Signature { get; set; } = null!;
    }

    /// <summary>
    /// Represents a digital signature for a request.
    /// </summary>
    public class RequestSignature
    {
        /// <summary>
        /// Gets or sets the account ID that signed the request.
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the cryptographic algorithm used.
        /// </summary>
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the signature value in base64 format.
        /// </summary>
        public string SignatureValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the nonce for replay protection.
        /// </summary>
        public string Nonce { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the signature was created.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Represents the result of signature verification.
    /// </summary>
    public class SignatureVerificationResult
    {
        /// <summary>
        /// Gets or sets whether the signature is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the account ID that was verified.
        /// </summary>
        public Guid? AccountId { get; set; }

        /// <summary>
        /// Gets or sets the algorithm used for verification.
        /// </summary>
        public string? Algorithm { get; set; }

        /// <summary>
        /// Gets or sets when the verification was performed.
        /// </summary>
        public DateTime VerifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the error code if verification failed.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error message if verification failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents the result of public key retrieval.
    /// </summary>
    public class PublicKeyResult
    {
        /// <summary>
        /// Gets or sets whether the public key retrieval was successful.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the public key in base64 format.
        /// </summary>
        public string? PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the algorithm.
        /// </summary>
        public string? Algorithm { get; set; }

        /// <summary>
        /// Gets or sets the public key hash.
        /// </summary>
        public string? PublicKeyHash { get; set; }

        /// <summary>
        /// Gets or sets the error code if retrieval failed.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error message if retrieval failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents verification statistics.
    /// </summary>
    public class VerificationStats
    {
        /// <summary>
        /// Gets or sets the total verification attempts.
        /// </summary>
        public long TotalAttempts { get; set; }

        /// <summary>
        /// Gets or sets the number of successful verifications.
        /// </summary>
        public long SuccessfulVerifications { get; set; }

        /// <summary>
        /// Gets or sets the number of failed verifications.
        /// </summary>
        public long FailedVerifications { get; set; }

        /// <summary>
        /// Gets or sets the number of detected replay attacks.
        /// </summary>
        public long ReplayAttacks { get; set; }

        /// <summary>
        /// Gets or sets the success rate as a percentage.
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Gets or sets the period start.
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Gets or sets the period end.
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Gets or sets the verifications per second.
        /// </summary>
        public double VerificationsPerSecond { get; set; }
    }

    #endregion
}
