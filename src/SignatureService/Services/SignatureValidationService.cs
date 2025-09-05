using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Cryptography.Models;
using SignatureService.Models;

namespace SignatureService.Services;

/// <summary>
/// Core signature validation service leveraging QuantumLedger's cryptographic infrastructure
/// </summary>
public class SignatureValidationService
{
    private readonly IEnumerable<ISignatureProvider> _signatureProviders;
    private readonly IKeyManager _keyManager;
    private readonly NonceTrackingService _nonceTracker;
    private readonly IDistributedCache _cache;
    private readonly ILogger<SignatureValidationService> _logger;

    public SignatureValidationService(
        IEnumerable<ISignatureProvider> signatureProviders,
        IKeyManager keyManager,
        NonceTrackingService nonceTracker,
        IDistributedCache cache,
        ILogger<SignatureValidationService> logger)
    {
        _signatureProviders = signatureProviders ?? throw new ArgumentNullException(nameof(signatureProviders));
        _keyManager = keyManager ?? throw new ArgumentNullException(nameof(keyManager));
        _nonceTracker = nonceTracker ?? throw new ArgumentNullException(nameof(nonceTracker));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates a universal signature request with replay protection
    /// Target: ≤1 second response time
    /// </summary>
    public async Task<SignatureValidationResult> ValidateSignatureAsync(
        UniversalSignatureValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var validationId = Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation("Starting signature validation for account {AccountId}, operation {Operation}", 
                request.AccountId, request.Operation);

            // 1. Input validation (≤50ms)
            var inputValidation = ValidateInput(request);
            if (!inputValidation.IsValid)
            {
                return SignatureValidationResult.Failed(inputValidation.Error);
            }

            // 2. Timestamp validation (≤50ms)
            if (!IsTimestampValid(request.Timestamp))
            {
                return SignatureValidationResult.Failed("Request timestamp is outside acceptable window (5 minutes)");
            }

            // 3. Nonce and replay protection (≤200ms)
            var nonceValidation = await _nonceTracker.ValidateNonceAsync(
                request.AccountId, request.Nonce, request.SequenceNumber, request.Timestamp);
            
            if (!nonceValidation.IsValid)
            {
                return SignatureValidationResult.Failed($"Nonce validation failed: {nonceValidation.Error}");
            }

            // 4. Get public key with caching (≤100ms)
            var publicKey = await GetPublicKeyWithCachingAsync(request.AccountId, request.Algorithm);
            if (publicKey == null || publicKey.Length == 0)
            {
                return SignatureValidationResult.Failed($"Public key not found for account {request.AccountId} with algorithm {request.Algorithm}");
            }

            // 5. Signature verification (≤300ms)
            var messageBytes = CreateCanonicalMessage(request);
            var signatureBytes = Convert.FromBase64String(request.Signature);
            
            var signatureProvider = GetSignatureProvider(request.Algorithm);
            if (signatureProvider == null)
            {
                return SignatureValidationResult.Failed($"Unsupported signature algorithm: {request.Algorithm}");
            }

            var isValidSignature = await signatureProvider.VerifyAsync(messageBytes, signatureBytes, publicKey);
            
            if (!isValidSignature)
            {
                return SignatureValidationResult.Failed("Signature verification failed");
            }

            // 6. Record nonce (async, non-blocking ≤50ms)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _nonceTracker.RecordNonceAsync(request.AccountId, request.Nonce, request.SequenceNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to record nonce for account {AccountId}", request.AccountId);
                }
            }, cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Signature validation successful for account {AccountId} in {ElapsedMs}ms", 
                request.AccountId, stopwatch.ElapsedMilliseconds);

            return new SignatureValidationResult
            {
                IsValid = true,
                ValidationId = validationId,
                ValidatedAt = DateTime.UtcNow,
                ProcessingTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object>
                {
                    ["system_id"] = request.SystemId,
                    ["service_id"] = request.ServiceId,
                    ["algorithm"] = request.Algorithm,
                    ["nonce"] = request.Nonce
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during signature validation for account {AccountId}", request.AccountId);
            
            return SignatureValidationResult.Failed($"Internal validation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates a dual signature (classic + quantum) from QuantumLedger
    /// Target: ≤1 second response time
    /// </summary>
    public async Task<DualSignatureValidationResult> ValidateDualSignatureAsync(
        DualSignatureValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var validationId = Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation("Starting dual signature validation for account {AccountId}", request.AccountId);

            // 1. Input validation
            if (string.IsNullOrWhiteSpace(request.AccountId))
            {
                return new DualSignatureValidationResult
                {
                    Success = false,
                    ErrorMessage = "Account ID is required",
                    ValidatedAt = DateTime.UtcNow
                };
            }

            // 2. Timestamp validation
            if (!IsTimestampValid(request.Timestamp))
            {
                return new DualSignatureValidationResult
                {
                    Success = false,
                    ErrorMessage = "Request timestamp is outside acceptable window",
                    ValidatedAt = DateTime.UtcNow
                };
            }

            // 3. Nonce validation
            var nonceValidation = await _nonceTracker.ValidateNonceAsync(
                request.AccountId, request.Nonce, request.SequenceNumber, request.Timestamp);
            
            if (!nonceValidation.IsValid)
            {
                return new DualSignatureValidationResult
                {
                    Success = false,
                    ErrorMessage = $"Nonce validation failed: {nonceValidation.Error}",
                    ValidatedAt = DateTime.UtcNow
                };
            }

            // 4. Validate classic signature
            bool classicValid = false;
            if (!string.IsNullOrWhiteSpace(request.Signature.ClassicSignature))
            {
                var classicKey = await GetPublicKeyByIdAsync(request.Signature.ClassicKeyId);
                if (classicKey != null)
                {
                    var classicProvider = GetSignatureProvider("EC256"); // Assuming EC256 for classic
                    if (classicProvider != null)
                    {
                        var classicSignatureBytes = request.Signature.GetClassicSignatureBytes();
                        classicValid = await classicProvider.VerifyAsync(request.OperationData, classicSignatureBytes, classicKey);
                    }
                }
            }

            // 5. Validate quantum signature
            bool quantumValid = false;
            if (!string.IsNullOrWhiteSpace(request.Signature.QuantumSignature))
            {
                var quantumKey = await GetPublicKeyByIdAsync(request.Signature.QuantumKeyId);
                if (quantumKey != null)
                {
                    var quantumProvider = GetSignatureProvider("Dilithium"); // Default quantum algorithm
                    if (quantumProvider != null)
                    {
                        var quantumSignatureBytes = request.Signature.GetQuantumSignatureBytes();
                        quantumValid = await quantumProvider.VerifyAsync(request.OperationData, quantumSignatureBytes, quantumKey);
                    }
                }
            }

            // 6. Record nonce (async)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _nonceTracker.RecordNonceAsync(request.AccountId, request.Nonce, request.SequenceNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to record nonce for account {AccountId}", request.AccountId);
                }
            }, cancellationToken);

            stopwatch.Stop();

            var result = new DualSignatureValidationResult
            {
                Success = classicValid && quantumValid,
                ClassicValid = classicValid,
                QuantumValid = quantumValid,
                ValidationId = validationId,
                ValidatedAt = DateTime.UtcNow,
                ProcessingTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object>
                {
                    ["classic_key_id"] = request.Signature.ClassicKeyId,
                    ["quantum_key_id"] = request.Signature.QuantumKeyId,
                    ["nonce"] = request.Nonce
                }
            };

            if (!result.Success)
            {
                result.ErrorMessage = $"Dual signature validation failed - Classic: {classicValid}, Quantum: {quantumValid}";
            }

            _logger.LogInformation("Dual signature validation completed for account {AccountId} - Success: {Success}, Classic: {ClassicValid}, Quantum: {QuantumValid}, Time: {ElapsedMs}ms", 
                request.AccountId, result.Success, classicValid, quantumValid, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during dual signature validation for account {AccountId}", request.AccountId);
            
            return new DualSignatureValidationResult
            {
                Success = false,
                ErrorMessage = $"Internal validation error: {ex.Message}",
                ValidatedAt = DateTime.UtcNow,
                ProcessingTime = stopwatch.Elapsed
            };
        }
    }

    /// <summary>
    /// Validates input parameters
    /// </summary>
    private static SignatureValidationResult ValidateInput(UniversalSignatureValidationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountId))
            return SignatureValidationResult.Failed("Account ID is required");

        if (string.IsNullOrWhiteSpace(request.Operation))
            return SignatureValidationResult.Failed("Operation is required");

        if (string.IsNullOrWhiteSpace(request.Signature))
            return SignatureValidationResult.Failed("Signature is required");

        if (string.IsNullOrWhiteSpace(request.Algorithm))
            return SignatureValidationResult.Failed("Algorithm is required");

        if (string.IsNullOrWhiteSpace(request.Nonce))
            return SignatureValidationResult.Failed("Nonce is required");

        try
        {
            Convert.FromBase64String(request.Signature);
        }
        catch (FormatException)
        {
            return SignatureValidationResult.Failed("Invalid signature format (must be base64)");
        }

        return SignatureValidationResult.Valid();
    }

    /// <summary>
    /// Validates timestamp is within acceptable window (5 minutes)
    /// </summary>
    private static bool IsTimestampValid(DateTime timestamp)
    {
        var now = DateTime.UtcNow;
        var timeDifference = Math.Abs((now - timestamp).TotalMinutes);
        return timeDifference <= 5; // 5-minute window
    }

    /// <summary>
    /// Gets public key with multi-layer caching for performance
    /// </summary>
    private async Task<byte[]?> GetPublicKeyWithCachingAsync(string accountId, string algorithm)
    {
        var cacheKey = $"pubkey:{accountId}:{algorithm}";
        
        // Try cache first (≤50ms)
        var cachedKey = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedKey))
        {
            return Convert.FromBase64String(cachedKey);
        }

        // Get from key manager (≤100ms)
        try
        {
            var latestKeyId = await _keyManager.GetLatestKeyPairAsync(accountId, KeyCategory.Traditional);
            if (string.IsNullOrEmpty(latestKeyId))
            {
                return null;
            }

            var publicKey = await _keyManager.GetPublicKeyAsync(latestKeyId);
            
            // Cache for 1 hour
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            
            await _cache.SetStringAsync(cacheKey, Convert.ToBase64String(publicKey), cacheOptions);
            
            return publicKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get public key for account {AccountId} with algorithm {Algorithm}", accountId, algorithm);
            return null;
        }
    }

    /// <summary>
    /// Gets public key by key ID
    /// </summary>
    private async Task<byte[]?> GetPublicKeyByIdAsync(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            return null;

        try
        {
            return await _keyManager.GetPublicKeyAsync(keyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get public key for key ID {KeyId}", keyId);
            return null;
        }
    }

    /// <summary>
    /// Gets signature provider for algorithm
    /// </summary>
    private ISignatureProvider? GetSignatureProvider(string algorithm)
    {
        return _signatureProviders.FirstOrDefault(p => 
            string.Equals(p.Algorithm, algorithm, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Creates canonical message for signature verification
    /// </summary>
    private static byte[] CreateCanonicalMessage(UniversalSignatureValidationRequest request)
    {
        var canonicalData = new
        {
            account_id = request.AccountId,
            operation = request.Operation,
            operation_data = request.OperationData,
            nonce = request.Nonce,
            sequence_number = request.SequenceNumber,
            timestamp = request.Timestamp.ToString("O") // ISO 8601 format
        };

        var json = JsonSerializer.Serialize(canonicalData, new JsonSerializerOptions
        {
            WriteIndented = false // No whitespace for consistent bytes
        });

        return Encoding.UTF8.GetBytes(json);
    }
}
