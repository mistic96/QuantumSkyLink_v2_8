using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;
using InfrastructureService.Services.Interfaces;
using Microsoft.Extensions.Logging;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Cryptography.Providers;
using QuantumLedger.Cryptography.PQC.Providers;

namespace InfrastructureService.Services;

/// <summary>
/// Implementation of RAGS (Robust Anti-replay Governance Signature) validation system
/// Provides quantum-resistant signature generation and validation with nonce replay protection
/// </summary>
public class SignatureValidationService : ISignatureValidationService
{
    private readonly IServiceRegistrationService _serviceRegistrationService;
    private readonly IBlockchainAddressService _blockchainAddressService;
    private readonly ILogger<SignatureValidationService> _logger;

    // Signature providers for different algorithms
    private readonly Dictionary<string, ISignatureProvider> _signatureProviders;

    // Nonce tracking for replay protection (in-memory for Phase 4)
    private readonly ConcurrentDictionary<string, NonceRecord> _usedNonces;

    // Performance metrics tracking
    private readonly ConcurrentQueue<SignatureOperation> _signatureOperations;
    private readonly object _metricsLock = new();

    // Constants
    private const int MaxNonceAge = 24; // Hours
    private const int NonceLength = 32; // Bytes
    private const int MaxMetricsHistory = 10000;

    public SignatureValidationService(
        IServiceRegistrationService serviceRegistrationService,
        IBlockchainAddressService blockchainAddressService,
        ILogger<SignatureValidationService> logger)
    {
        _serviceRegistrationService = serviceRegistrationService;
        _blockchainAddressService = blockchainAddressService;
        _logger = logger;

        _usedNonces = new ConcurrentDictionary<string, NonceRecord>();
        _signatureOperations = new ConcurrentQueue<SignatureOperation>();

        // Initialize signature providers with proper logger types
        _signatureProviders = new Dictionary<string, ISignatureProvider>();
        
        // For Phase 4, we'll create mock providers since we don't have actual keys stored
        // In production, these would be properly initialized with the correct logger types
        try
        {
            // Note: Using mock implementations for Phase 4 testing
            _signatureProviders["DILITHIUM"] = new MockSignatureProvider("DILITHIUM");
            _signatureProviders["FALCON"] = new MockSignatureProvider("FALCON");
            _signatureProviders["EC256"] = new MockSignatureProvider("EC256");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize signature providers, using mock implementations");
            _signatureProviders["DILITHIUM"] = new MockSignatureProvider("DILITHIUM");
            _signatureProviders["FALCON"] = new MockSignatureProvider("FALCON");
            _signatureProviders["EC256"] = new MockSignatureProvider("EC256");
        }

        _logger.LogInformation("RAGS Signature Validation Service initialized with {AlgorithmCount} algorithms", 
            _signatureProviders.Count);
    }

    public async Task<GenerateSignatureResponse> GenerateSignatureAsync(GenerateSignatureRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new GenerateSignatureResponse
        {
            ServiceName = request.ServiceName,
            Algorithm = request.Algorithm,
            GeneratedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Generating signature for service {ServiceName} using {Algorithm}", 
                request.ServiceName, request.Algorithm);

            // Validate service exists and has required keys
            if (!await ValidateServiceReadinessAsync(request.ServiceName, request.Algorithm, cancellationToken))
            {
                response.Success = false;
                response.ErrorMessage = $"Service {request.ServiceName} is not ready for {request.Algorithm} signature operations";
                return response;
            }

            // Get or generate nonce
            var nonce = request.Nonce ?? await GenerateNonceAsync(request.ServiceName, cancellationToken);
            response.Nonce = nonce;

            // Get blockchain address for the service
            var blockchainAddress = await GetServiceBlockchainAddressAsync(request.ServiceName, request.BlockchainAddress, cancellationToken);
            response.BlockchainAddress = blockchainAddress;

            // Get service keys
            var serviceKeys = await GetServiceKeysAsync(request.ServiceName, request.Algorithm, cancellationToken);
            response.PublicKey = Convert.ToBase64String(serviceKeys.PublicKey);

            // Create message hash with context
            var messageContext = CreateMessageContext(request.Message, nonce, blockchainAddress, request.Metadata);
            var messageHash = ComputeMessageHash(messageContext);
            response.MessageHash = Convert.ToBase64String(messageHash);

            // Generate signature
            var signatureProvider = _signatureProviders[request.Algorithm.ToUpperInvariant()];
            var signatureBytes = await signatureProvider.SignAsync(messageHash, serviceKeys.PrivateKey);
            response.Signature = Convert.ToBase64String(signatureBytes);

            // Mark nonce as used
            await MarkNonceAsUsedAsync(request.ServiceName, nonce, cancellationToken);

            // Set metadata
            response.Metadata = request.Metadata;
            response.Success = true;

            stopwatch.Stop();
            response.GenerationTime = stopwatch.Elapsed;

            // Track metrics
            TrackSignatureOperation(new SignatureOperation
            {
                Type = "Generation",
                ServiceName = request.ServiceName,
                Algorithm = request.Algorithm,
                Success = true,
                Duration = stopwatch.Elapsed,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Successfully generated signature for {ServiceName} in {Duration}ms", 
                request.ServiceName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.Success = false;
            response.ErrorMessage = ex.Message;
            response.GenerationTime = stopwatch.Elapsed;

            TrackSignatureOperation(new SignatureOperation
            {
                Type = "Generation",
                ServiceName = request.ServiceName,
                Algorithm = request.Algorithm,
                Success = false,
                Duration = stopwatch.Elapsed,
                Timestamp = DateTime.UtcNow,
                ErrorMessage = ex.Message
            });

            _logger.LogError(ex, "Failed to generate signature for {ServiceName}", request.ServiceName);
            return response;
        }
    }

    public async Task<ValidateSignatureResponse> ValidateSignatureAsync(ValidateSignatureRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new ValidateSignatureResponse
        {
            ServiceName = request.ServiceName,
            Algorithm = request.Algorithm,
            Nonce = request.Nonce,
            BlockchainAddress = request.BlockchainAddress,
            ValidatedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Validating signature for service {ServiceName} using {Algorithm}", 
                request.ServiceName, request.Algorithm);

            var validationDetails = new SignatureValidationDetails();
            response.ValidationDetails = validationDetails;

            // Check nonce for replay protection
            var nonceCheck = await CheckNonceAsync(new CheckNonceRequest 
            { 
                ServiceName = request.ServiceName, 
                Nonce = request.Nonce 
            }, cancellationToken);

            response.IsNonceReused = nonceCheck.IsUsed;
            validationDetails.NonceValid = !nonceCheck.IsUsed;

            if (nonceCheck.IsUsed)
            {
                validationDetails.ValidationNotes.Add($"Nonce has been used {nonceCheck.UsageCount} times. First used at {nonceCheck.FirstUsedAt}");
            }

            // Validate signature format
            try
            {
                Convert.FromBase64String(request.Signature);
                validationDetails.SignatureFormatValid = true;
            }
            catch
            {
                validationDetails.SignatureFormatValid = false;
                validationDetails.ValidationNotes.Add("Invalid signature format - not valid base64");
            }

            // Validate service and get keys
            var serviceKeys = await GetServiceKeysAsync(request.ServiceName, request.Algorithm, cancellationToken);
            validationDetails.PublicKeyValid = serviceKeys.PublicKey != null;

            // Validate blockchain address
            validationDetails.BlockchainAddressValid = await ValidateServiceBlockchainAddressAsync(
                request.ServiceName, request.BlockchainAddress, cancellationToken);

            // Create message context and hash
            var messageContext = CreateMessageContext(request.Message, request.Nonce, request.BlockchainAddress, request.Metadata);
            var messageHash = ComputeMessageHash(messageContext);
            validationDetails.MessageHashValid = true;

            // Perform cryptographic signature validation
            if (validationDetails.SignatureFormatValid && validationDetails.PublicKeyValid)
            {
                try
                {
                    var signatureProvider = _signatureProviders[request.Algorithm.ToUpperInvariant()];
                    var signatureBytes = Convert.FromBase64String(request.Signature);
                    
                    validationDetails.CryptographicSignatureValid = await signatureProvider.VerifyAsync(
                        messageHash, signatureBytes, serviceKeys.PublicKey);
                }
                catch (Exception ex)
                {
                    validationDetails.CryptographicSignatureValid = false;
                    validationDetails.ValidationNotes.Add($"Cryptographic validation failed: {ex.Message}");
                }
            }

            // Overall validation result
            response.IsValid = validationDetails.SignatureFormatValid &&
                              validationDetails.PublicKeyValid &&
                              validationDetails.MessageHashValid &&
                              validationDetails.CryptographicSignatureValid &&
                              validationDetails.NonceValid &&
                              validationDetails.BlockchainAddressValid;

            stopwatch.Stop();
            response.ValidationTime = stopwatch.Elapsed;

            // Track metrics
            TrackSignatureOperation(new SignatureOperation
            {
                Type = "Validation",
                ServiceName = request.ServiceName,
                Algorithm = request.Algorithm,
                Success = response.IsValid,
                Duration = stopwatch.Elapsed,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Signature validation for {ServiceName} completed: {IsValid} in {Duration}ms", 
                request.ServiceName, response.IsValid, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.IsValid = false;
            response.ErrorMessage = ex.Message;
            response.ValidationTime = stopwatch.Elapsed;

            TrackSignatureOperation(new SignatureOperation
            {
                Type = "Validation",
                ServiceName = request.ServiceName,
                Algorithm = request.Algorithm,
                Success = false,
                Duration = stopwatch.Elapsed,
                Timestamp = DateTime.UtcNow,
                ErrorMessage = ex.Message
            });

            _logger.LogError(ex, "Failed to validate signature for {ServiceName}", request.ServiceName);
            return response;
        }
    }

    public async Task<BulkGenerateSignatureResponse> BulkGenerateSignaturesAsync(BulkGenerateSignatureRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new BulkGenerateSignatureResponse
        {
            Algorithm = request.Algorithm,
            CompletedAt = DateTime.UtcNow
        };

        try
        {
            // Get service names to process
            var serviceNames = request.ServiceNames ?? (await _serviceRegistrationService.GetAllServicesAsync())
                .Select(s => s.ServiceName).ToList();

            response.TotalRequested = serviceNames.Count;

            _logger.LogInformation("Starting bulk signature generation for {ServiceCount} services using {Algorithm}", 
                serviceNames.Count, request.Algorithm);

            // Generate signatures in parallel
            var tasks = serviceNames.Select(async serviceName =>
            {
                var generateRequest = new GenerateSignatureRequest
                {
                    ServiceName = serviceName,
                    Message = request.Message,
                    Algorithm = request.Algorithm,
                    Metadata = request.Metadata
                };

                return await GenerateSignatureAsync(generateRequest, cancellationToken);
            });

            var results = await Task.WhenAll(tasks);
            response.Signatures = results.ToList();

            response.SuccessfullyGenerated = results.Count(r => r.Success);
            response.Failed = results.Count(r => !r.Success);

            // Collect errors
            response.Errors = results
                .Where(r => !r.Success && !string.IsNullOrEmpty(r.ErrorMessage))
                .Select(r => $"{r.ServiceName}: {r.ErrorMessage}")
                .ToList();

            stopwatch.Stop();
            response.TotalGenerationTime = stopwatch.Elapsed;
            response.AverageGenerationTime = TimeSpan.FromMilliseconds(
                stopwatch.ElapsedMilliseconds / (double)serviceNames.Count);

            _logger.LogInformation("Bulk signature generation completed: {Successful}/{Total} successful in {Duration}ms", 
                response.SuccessfullyGenerated, response.TotalRequested, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.TotalGenerationTime = stopwatch.Elapsed;
            response.Errors.Add($"Bulk operation failed: {ex.Message}");

            _logger.LogError(ex, "Bulk signature generation failed");
            return response;
        }
    }

    public async Task<CheckNonceResponse> CheckNonceAsync(CheckNonceRequest request, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For async interface compliance

        var nonceKey = $"{request.ServiceName}:{request.Nonce}";
        var response = new CheckNonceResponse
        {
            ServiceName = request.ServiceName,
            Nonce = request.Nonce,
            CheckedAt = DateTime.UtcNow
        };

        if (_usedNonces.TryGetValue(nonceKey, out var nonceRecord))
        {
            response.IsUsed = true;
            response.FirstUsedAt = nonceRecord.FirstUsed;
            response.UsageCount = nonceRecord.UsageCount;
            response.IsValidForUse = false;
        }
        else
        {
            response.IsUsed = false;
            response.UsageCount = 0;
            response.IsValidForUse = true;
        }

        return response;
    }

    public async Task<SignatureMetricsResponse> GetSignatureMetricsAsync(GetSignatureMetricsRequest request, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For async interface compliance

        var operations = _signatureOperations.ToList();

        // Apply filters
        if (!string.IsNullOrEmpty(request.ServiceName))
        {
            operations = operations.Where(o => o.ServiceName.Equals(request.ServiceName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrEmpty(request.Algorithm))
        {
            operations = operations.Where(o => o.Algorithm.Equals(request.Algorithm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (request.StartDate.HasValue)
        {
            operations = operations.Where(o => o.Timestamp >= request.StartDate.Value).ToList();
        }

        if (request.EndDate.HasValue)
        {
            operations = operations.Where(o => o.Timestamp <= request.EndDate.Value).ToList();
        }

        var response = new SignatureMetricsResponse
        {
            ServiceNameFilter = request.ServiceName,
            AlgorithmFilter = request.Algorithm,
            DateRange = new DateRange
            {
                StartDate = request.StartDate ?? operations.MinBy(o => o.Timestamp)?.Timestamp ?? DateTime.UtcNow,
                EndDate = request.EndDate ?? operations.MaxBy(o => o.Timestamp)?.Timestamp ?? DateTime.UtcNow
            },
            GeneratedAt = DateTime.UtcNow
        };

        var generations = operations.Where(o => o.Type == "Generation").ToList();
        var validations = operations.Where(o => o.Type == "Validation").ToList();

        response.TotalSignaturesGenerated = generations.Count;
        response.TotalSignaturesValidated = validations.Count;
        response.ValidSignatures = validations.Count(v => v.Success);
        response.InvalidSignatures = validations.Count(v => !v.Success);
        response.ReplayAttacksDetected = _usedNonces.Count(n => n.Value.UsageCount > 1);

        if (generations.Any())
        {
            response.AverageGenerationTime = TimeSpan.FromMilliseconds(
                generations.Average(g => g.Duration.TotalMilliseconds));
        }

        if (validations.Any())
        {
            response.AverageValidationTime = TimeSpan.FromMilliseconds(
                validations.Average(v => v.Duration.TotalMilliseconds));
        }

        // Algorithm metrics
        response.AlgorithmMetrics = operations
            .GroupBy(o => o.Algorithm)
            .Select(g => new AlgorithmMetrics
            {
                Algorithm = g.Key,
                SignaturesGenerated = g.Count(o => o.Type == "Generation"),
                SignaturesValidated = g.Count(o => o.Type == "Validation"),
                ValidSignatures = g.Count(o => o.Type == "Validation" && o.Success),
                AverageGenerationTime = TimeSpan.FromMilliseconds(
                    g.Where(o => o.Type == "Generation").DefaultIfEmpty().Average(o => o?.Duration.TotalMilliseconds ?? 0)),
                AverageValidationTime = TimeSpan.FromMilliseconds(
                    g.Where(o => o.Type == "Validation").DefaultIfEmpty().Average(o => o?.Duration.TotalMilliseconds ?? 0))
            }).ToList();

        // Service metrics
        response.ServiceMetrics = operations
            .GroupBy(o => o.ServiceName)
            .Select(g => new ServiceMetrics
            {
                ServiceName = g.Key,
                SignaturesGenerated = g.Count(o => o.Type == "Generation"),
                SignaturesValidated = g.Count(o => o.Type == "Validation"),
                ValidSignatures = g.Count(o => o.Type == "Validation" && o.Success),
                ReplayAttacksDetected = _usedNonces.Count(n => n.Key.StartsWith($"{g.Key}:") && n.Value.UsageCount > 1)
            }).ToList();

        return response;
    }

    public async Task<List<string>> GetSupportedAlgorithmsAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For async interface compliance
        return _signatureProviders.Keys.ToList();
    }

    public async Task<string> GenerateNonceAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For async interface compliance

        // Generate cryptographically secure random nonce
        var nonceBytes = new byte[NonceLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(nonceBytes);
        }

        // Include timestamp and service name for uniqueness
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nonceData = $"{serviceName}:{timestamp}:{Convert.ToBase64String(nonceBytes)}";

        // Hash the nonce data for consistent length
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(nonceData));
            return Convert.ToBase64String(hashBytes);
        }
    }

    public async Task<bool> ValidateServiceReadinessAsync(string serviceName, string algorithm, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if service is registered
            var services = await _serviceRegistrationService.GetAllServicesAsync();
            var service = services.FirstOrDefault(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

            if (service == null)
            {
                _logger.LogWarning("Service {ServiceName} not found in registration", serviceName);
                return false;
            }

            // Check if algorithm is supported
            if (!_signatureProviders.ContainsKey(algorithm.ToUpperInvariant()))
            {
                _logger.LogWarning("Algorithm {Algorithm} not supported", algorithm);
                return false;
            }

            // Check if service has required keys for the algorithm
            // For Phase 4, we'll check for key IDs since actual keys aren't stored in the model
            var hasKeys = algorithm.ToUpperInvariant() switch
            {
                "DILITHIUM" => !string.IsNullOrEmpty(service.DilithiumKeyId),
                "FALCON" => !string.IsNullOrEmpty(service.FalconKeyId),
                "EC256" => !string.IsNullOrEmpty(service.EC256KeyId),
                _ => false
            };

            if (!hasKeys)
            {
                _logger.LogWarning("Service {ServiceName} missing {Algorithm} keys", serviceName, algorithm);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating service readiness for {ServiceName}", serviceName);
            return false;
        }
    }

    public async Task<int> ClearExpiredNoncesAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For async interface compliance

        var cutoffTime = DateTime.UtcNow - maxAge;
        var expiredKeys = _usedNonces
            .Where(kvp => kvp.Value.FirstUsed < cutoffTime)
            .Select(kvp => kvp.Key)
            .ToList();

        var clearedCount = 0;
        foreach (var key in expiredKeys)
        {
            if (_usedNonces.TryRemove(key, out _))
            {
                clearedCount++;
            }
        }

        _logger.LogInformation("Cleared {Count} expired nonces older than {MaxAge}", clearedCount, maxAge);
        return clearedCount;
    }

    public async Task<Dictionary<string, object>> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For async interface compliance

        var status = new Dictionary<string, object>
        {
            ["service"] = "SignatureValidationService",
            ["status"] = "healthy",
            ["timestamp"] = DateTime.UtcNow,
            ["supported_algorithms"] = _signatureProviders.Keys.ToList(),
            ["active_nonces"] = _usedNonces.Count,
            ["metrics_history_count"] = _signatureOperations.Count
        };

        // Test each signature provider
        var providerStatus = new Dictionary<string, bool>();
        foreach (var provider in _signatureProviders)
        {
            try
            {
                // Simple test to verify provider is working
                providerStatus[provider.Key] = !string.IsNullOrEmpty(provider.Value.Algorithm);
            }
            catch
            {
                providerStatus[provider.Key] = false;
            }
        }

        status["provider_status"] = providerStatus;
        status["overall_healthy"] = providerStatus.Values.All(v => v);

        return status;
    }

    // Private helper methods

    private async Task<string> GetServiceBlockchainAddressAsync(string serviceName, string? requestedAddress, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(requestedAddress))
        {
            return requestedAddress;
        }

        // Get service's primary blockchain address
        var addressRequest = new GenerateAddressRequest
        {
            ServiceName = serviceName,
            NetworkType = "MULTICHAIN" // Default to MultiChain
        };

        var addressResponse = await _blockchainAddressService.GenerateAddressAsync(addressRequest, cancellationToken);
        return addressResponse.Address;
    }

    private async Task<(byte[] PublicKey, byte[] PrivateKey)> GetServiceKeysAsync(string serviceName, string algorithm, CancellationToken cancellationToken)
    {
        var services = await _serviceRegistrationService.GetAllServicesAsync();
        var service = services.FirstOrDefault(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

        if (service == null)
        {
            throw new InvalidOperationException($"Service {serviceName} not found");
        }

        // For Phase 4, we'll generate mock keys based on the key IDs
        // In production, these would be retrieved from a secure key store
        return algorithm.ToUpperInvariant() switch
        {
            "DILITHIUM" => GenerateMockKeyPair(service.DilithiumKeyId, "DILITHIUM"),
            "FALCON" => GenerateMockKeyPair(service.FalconKeyId, "FALCON"),
            "EC256" => GenerateMockKeyPair(service.EC256KeyId, "EC256"),
            _ => throw new ArgumentException($"Unsupported algorithm: {algorithm}")
        };
    }

    private (byte[] PublicKey, byte[] PrivateKey) GenerateMockKeyPair(string keyId, string algorithm)
    {
        // Generate deterministic mock keys based on keyId for Phase 4 testing
        // In production, this would retrieve actual keys from secure storage
        var keyData = Encoding.UTF8.GetBytes($"{keyId}:{algorithm}");
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(keyData);
            
            // Create mock keys of appropriate sizes for each algorithm
            var (pubKeySize, privKeySize) = algorithm switch
            {
                "DILITHIUM" => (1952, 4000), // Approximate Dilithium-3 key sizes
                "FALCON" => (897, 1281),     // Falcon-512 key sizes
                "EC256" => (65, 32),         // ECDSA P-256 key sizes
                _ => (64, 64)
            };

            var publicKey = new byte[pubKeySize];
            var privateKey = new byte[privKeySize];

            // Fill with deterministic data based on hash
            for (int i = 0; i < publicKey.Length; i++)
            {
                publicKey[i] = hash[i % hash.Length];
            }
            
            for (int i = 0; i < privateKey.Length; i++)
            {
                privateKey[i] = hash[(i + publicKey.Length) % hash.Length];
            }

            return (publicKey, privateKey);
        }
    }

    private async Task<bool> ValidateServiceBlockchainAddressAsync(string serviceName, string blockchainAddress, CancellationToken cancellationToken)
    {
        try
        {
            // For Phase 4, we'll do basic validation
            // In production, this would verify the address belongs to the service
            return !string.IsNullOrEmpty(blockchainAddress) && blockchainAddress.Length > 10;
        }
        catch
        {
            return false;
        }
    }

    private string CreateMessageContext(string message, string nonce, string blockchainAddress, Dictionary<string, string>? metadata)
    {
        var context = new
        {
            message,
            nonce,
            blockchainAddress,
            metadata = metadata ?? new Dictionary<string, string>(),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        return JsonSerializer.Serialize(context);
    }

    private byte[] ComputeMessageHash(string messageContext)
    {
        using (var sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(messageContext));
        }
    }

    private async Task MarkNonceAsUsedAsync(string serviceName, string nonce, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; // For async interface compliance

        var nonceKey = $"{serviceName}:{nonce}";
        _usedNonces.AddOrUpdate(nonceKey,
            new NonceRecord { FirstUsed = DateTime.UtcNow, UsageCount = 1 },
            (key, existing) => new NonceRecord { FirstUsed = existing.FirstUsed, UsageCount = existing.UsageCount + 1 });
    }

    private void TrackSignatureOperation(SignatureOperation operation)
    {
        _signatureOperations.Enqueue(operation);

        // Keep only recent operations to prevent memory growth
        while (_signatureOperations.Count > MaxMetricsHistory)
        {
            _signatureOperations.TryDequeue(out _);
        }
    }

    // Helper classes for internal tracking
    private class NonceRecord
    {
        public DateTime FirstUsed { get; set; }
        public int UsageCount { get; set; }
    }

    private class SignatureOperation
    {
        public string Type { get; set; } = string.Empty; // "Generation" or "Validation"
        public string ServiceName { get; set; } = string.Empty;
        public string Algorithm { get; set; } = string.Empty;
        public bool Success { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime Timestamp { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Mock signature provider for Phase 4 testing
    /// In production, this would be replaced with actual quantum-resistant signature providers
    /// </summary>
    private class MockSignatureProvider : ISignatureProvider
    {
        public string Algorithm { get; }

        public MockSignatureProvider(string algorithm)
        {
            Algorithm = algorithm;
        }

        public ValueTask<byte[]> SignAsync(byte[] message, byte[] privateKey)
        {
            // Create a deterministic mock signature based on message and private key
            var signatureData = new List<byte>();
            signatureData.AddRange(message);
            signatureData.AddRange(privateKey);
            signatureData.AddRange(Encoding.UTF8.GetBytes(Algorithm));

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(signatureData.ToArray());
                
                // Create signature of appropriate size for the algorithm
                var signatureSize = Algorithm switch
                {
                    "DILITHIUM" => 3293, // Dilithium-3 signature size
                    "FALCON" => 690,     // Falcon-512 signature size
                    "EC256" => 64,       // ECDSA P-256 signature size
                    _ => 64
                };

                var signature = new byte[signatureSize];
                for (int i = 0; i < signature.Length; i++)
                {
                    signature[i] = hash[i % hash.Length];
                }

                return ValueTask.FromResult(signature);
            }
        }

        public ValueTask<bool> VerifyAsync(byte[] message, byte[] signature, byte[] publicKey)
        {
            try
            {
                // For mock verification, regenerate the expected signature and compare
                var signatureData = new List<byte>();
                signatureData.AddRange(message);
                
                // Generate mock private key from public key for verification
                using (var sha256 = SHA256.Create())
                {
                    var privateKeyHash = sha256.ComputeHash(publicKey);
                    var mockPrivateKey = new byte[privateKeyHash.Length * 2];
                    Array.Copy(privateKeyHash, 0, mockPrivateKey, 0, privateKeyHash.Length);
                    Array.Copy(privateKeyHash, 0, mockPrivateKey, privateKeyHash.Length, privateKeyHash.Length);
                    
                    signatureData.AddRange(mockPrivateKey);
                    signatureData.AddRange(Encoding.UTF8.GetBytes(Algorithm));

                    var expectedHash = sha256.ComputeHash(signatureData.ToArray());
                    
                    // Check if signature matches expected pattern
                    var signatureSize = Algorithm switch
                    {
                        "DILITHIUM" => 3293,
                        "FALCON" => 690,
                        "EC256" => 64,
                        _ => 64
                    };

                    if (signature.Length != signatureSize)
                    {
                        return ValueTask.FromResult(false);
                    }

                    // Verify signature pattern matches expected hash
                    for (int i = 0; i < Math.Min(expectedHash.Length, signature.Length); i++)
                    {
                        if (signature[i] != expectedHash[i % expectedHash.Length])
                        {
                            return ValueTask.FromResult(false);
                        }
                    }

                    return ValueTask.FromResult(true);
                }
            }
            catch
            {
                return ValueTask.FromResult(false);
            }
        }
    }
}
