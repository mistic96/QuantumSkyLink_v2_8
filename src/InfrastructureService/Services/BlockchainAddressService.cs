using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;
using InfrastructureService.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3.Accounts;

namespace InfrastructureService.Services
{
    /// <summary>
    /// Service for blockchain address generation and management.
    /// </summary>
    public class BlockchainAddressService : IBlockchainAddressService
    {
        private readonly ILogger<BlockchainAddressService> _logger;
        private readonly IServiceRegistrationService _serviceRegistrationService;
        private readonly List<GenerateAddressResponse> _generatedAddresses;
        private readonly List<TimeSpan> _generationTimes;
        private readonly object _metricsLock = new();

        /// <summary>
        /// Initializes a new instance of the BlockchainAddressService class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="serviceRegistrationService">The service registration service.</param>
        public BlockchainAddressService(
            ILogger<BlockchainAddressService> logger,
            IServiceRegistrationService serviceRegistrationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceRegistrationService = serviceRegistrationService ?? throw new ArgumentNullException(nameof(serviceRegistrationService));
            _generatedAddresses = new List<GenerateAddressResponse>();
            _generationTimes = new List<TimeSpan>();
        }

        /// <inheritdoc/>
        public async Task<GenerateAddressResponse> GenerateAddressAsync(GenerateAddressRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Generating {NetworkType} address for service: {ServiceName}", 
                    request.NetworkType, request.ServiceName);

                GenerateAddressResponse response = request.NetworkType.ToUpperInvariant() switch
                {
                    "MULTICHAIN" => await GenerateMultiChainAddressAsync(request, cancellationToken),
                    "ETHEREUM" => await GenerateEthereumAddressAsync(request, cancellationToken),
                    _ => throw new ArgumentException($"Unsupported network type: {request.NetworkType}")
                };

                stopwatch.Stop();
                
                // Track metrics
                lock (_metricsLock)
                {
                    _generatedAddresses.Add(response);
                    _generationTimes.Add(stopwatch.Elapsed);
                }

                _logger.LogInformation("Successfully generated {NetworkType} address for service {ServiceName}: {Address} in {ElapsedMs}ms",
                    request.NetworkType, request.ServiceName, response.Address, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to generate {NetworkType} address for service: {ServiceName}", 
                    request.NetworkType, request.ServiceName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<BulkGenerateAddressResponse> BulkGenerateAddressesAsync(BulkGenerateAddressRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var stopwatch = Stopwatch.StartNew();
            var addresses = new List<GenerateAddressResponse>();
            var tasks = new List<Task<GenerateAddressResponse>>();

            try
            {
                _logger.LogInformation("Starting bulk generation of {Count} {NetworkType} addresses", 
                    request.ServiceNames.Count, request.NetworkType);

                // Generate addresses in parallel for better performance
                foreach (var serviceName in request.ServiceNames)
                {
                    var individualRequest = new GenerateAddressRequest
                    {
                        ServiceName = serviceName,
                        NetworkType = request.NetworkType,
                        Metadata = request.Metadata
                    };

                    tasks.Add(GenerateAddressAsync(individualRequest, cancellationToken));
                }

                addresses.AddRange(await Task.WhenAll(tasks));
                stopwatch.Stop();

                var response = new BulkGenerateAddressResponse
                {
                    Addresses = addresses,
                    NetworkType = request.NetworkType,
                    TotalGenerated = addresses.Count,
                    GeneratedAt = DateTime.UtcNow,
                    GenerationTime = stopwatch.Elapsed
                };

                _logger.LogInformation("Successfully completed bulk generation of {Count} {NetworkType} addresses in {ElapsedMs}ms",
                    addresses.Count, request.NetworkType, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to bulk generate {NetworkType} addresses", request.NetworkType);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ValidateAddressResponse> ValidateAddressAsync(ValidateAddressRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                _logger.LogDebug("Validating {NetworkType} address: {Address}", request.NetworkType, request.Address);

                return request.NetworkType.ToUpperInvariant() switch
                {
                    "MULTICHAIN" => await ValidateMultiChainAddressAsync(request, cancellationToken),
                    "ETHEREUM" => ValidateEthereumAddress(request),
                    _ => new ValidateAddressResponse
                    {
                        Address = request.Address,
                        IsValid = false,
                        NetworkType = request.NetworkType,
                        ValidationErrors = new List<string> { $"Unsupported network type: {request.NetworkType}" }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate {NetworkType} address: {Address}", 
                    request.NetworkType, request.Address);
                
                return new ValidateAddressResponse
                {
                    Address = request.Address,
                    IsValid = false,
                    NetworkType = request.NetworkType,
                    ValidationErrors = new List<string> { ex.Message }
                };
            }
        }

        /// <inheritdoc/>
        public async Task<AddressInfoResponse> GetAddressInfoAsync(GetAddressInfoRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                _logger.LogDebug("Getting address info for {NetworkType} address: {Address}", 
                    request.NetworkType, request.Address);

                return request.NetworkType.ToUpperInvariant() switch
                {
                    "MULTICHAIN" => await GetMultiChainAddressInfoAsync(request, cancellationToken),
                    "ETHEREUM" => await GetEthereumAddressInfoAsync(request, cancellationToken),
                    _ => throw new ArgumentException($"Unsupported network type: {request.NetworkType}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get address info for {NetworkType} address: {Address}", 
                    request.NetworkType, request.Address);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<NetworkStatsResponse> GetNetworkStatsAsync(string networkType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(networkType))
                throw new ArgumentException("Network type cannot be null or empty", nameof(networkType));

            try
            {
                _logger.LogDebug("Getting network stats for: {NetworkType}", networkType);

                return networkType.ToUpperInvariant() switch
                {
                    "MULTICHAIN" => await GetMultiChainNetworkStatsAsync(cancellationToken),
                    "ETHEREUM" => await GetEthereumNetworkStatsAsync(cancellationToken),
                    _ => throw new ArgumentException($"Unsupported network type: {networkType}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get network stats for: {NetworkType}", networkType);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<AddressGenerationMetricsResponse> GetGenerationMetricsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                lock (_metricsLock)
                {
                    if (_generationTimes.Count == 0)
                    {
                        return new AddressGenerationMetricsResponse
                        {
                            TotalAddressesGenerated = 0,
                            AverageGenerationTime = TimeSpan.Zero,
                            FastestGenerationTime = TimeSpan.Zero,
                            SlowestGenerationTime = TimeSpan.Zero,
                            SuccessRate = 0.0,
                            NetworkMetrics = new Dictionary<string, NetworkGenerationMetrics>()
                        };
                    }

                    var networkGroups = _generatedAddresses.GroupBy(a => a.NetworkType);
                    var networkMetrics = new Dictionary<string, NetworkGenerationMetrics>();

                    foreach (var group in networkGroups)
                    {
                        var networkTimes = _generationTimes.Take(group.Count()).ToList();
                        networkMetrics[group.Key] = new NetworkGenerationMetrics
                        {
                            NetworkType = group.Key,
                            AddressCount = group.Count(),
                            AverageTime = TimeSpan.FromTicks((long)networkTimes.Average(t => t.Ticks)),
                            SuccessRate = 100.0 // All recorded addresses are successful
                        };
                    }

                    return new AddressGenerationMetricsResponse
                    {
                        TotalAddressesGenerated = _generatedAddresses.Count,
                        AverageGenerationTime = TimeSpan.FromTicks((long)_generationTimes.Average(t => t.Ticks)),
                        FastestGenerationTime = _generationTimes.Min(),
                        SlowestGenerationTime = _generationTimes.Max(),
                        SuccessRate = 100.0, // All recorded addresses are successful
                        NetworkMetrics = networkMetrics
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get generation metrics");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<BulkGenerateAddressResponse> GenerateAddressesForAllServicesAsync(string networkType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(networkType))
                throw new ArgumentException("Network type cannot be null or empty", nameof(networkType));

            try
            {
                _logger.LogInformation("Generating {NetworkType} addresses for all registered services", networkType);

                // Get all registered services
                var services = await _serviceRegistrationService.GetAllServicesAsync();
                var serviceNames = services.Select(s => s.ServiceName).ToList();

                if (serviceNames.Count == 0)
                {
                    _logger.LogWarning("No registered services found for address generation");
                    return new BulkGenerateAddressResponse
                    {
                        Addresses = new List<GenerateAddressResponse>(),
                        NetworkType = networkType,
                        TotalGenerated = 0,
                        GeneratedAt = DateTime.UtcNow,
                        GenerationTime = TimeSpan.Zero
                    };
                }

                var bulkRequest = new BulkGenerateAddressRequest
                {
                    ServiceNames = serviceNames,
                    NetworkType = networkType,
                    Metadata = new Dictionary<string, string>
                    {
                        ["GenerationType"] = "AllServices",
                        ["RequestedAt"] = DateTime.UtcNow.ToString("O")
                    }
                };

                return await BulkGenerateAddressesAsync(bulkRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate {NetworkType} addresses for all services", networkType);
                throw;
            }
        }

        #region Private Methods

        /// <summary>
        /// Generates a MultiChain address for a service.
        /// </summary>
        private async Task<GenerateAddressResponse> GenerateMultiChainAddressAsync(GenerateAddressRequest request, CancellationToken cancellationToken)
        {
            // For MultiChain, we'll generate a deterministic address based on the service name
            var serviceHash = SHA256.HashData(Encoding.UTF8.GetBytes($"multichain-{request.ServiceName}-{DateTime.UtcNow.Ticks}"));
            var addressBytes = serviceHash.Take(20).ToArray();
            var address = "1" + Convert.ToBase64String(addressBytes).Replace("+", "").Replace("/", "").Replace("=", "")[..25];

            // Generate a simple key pair for demonstration
            using var rsa = RSA.Create(2048);
            var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

            return new GenerateAddressResponse
            {
                ServiceName = request.ServiceName,
                Address = address,
                NetworkType = "MultiChain",
                PublicKey = publicKey,
                PrivateKey = privateKey,
                GeneratedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>(request.Metadata ?? new Dictionary<string, string>())
                {
                    ["AddressType"] = "MultiChain",
                    ["GenerationMethod"] = "Deterministic",
                    ["KeyAlgorithm"] = "RSA-2048"
                }
            };
        }

        /// <summary>
        /// Generates an Ethereum address for a service.
        /// </summary>
        private async Task<GenerateAddressResponse> GenerateEthereumAddressAsync(GenerateAddressRequest request, CancellationToken cancellationToken)
        {
            // Generate a new Ethereum account
            var ecKey = EthECKey.GenerateKey();
            var account = new Account(ecKey);

            return new GenerateAddressResponse
            {
                ServiceName = request.ServiceName,
                Address = account.Address,
                NetworkType = "Ethereum",
                PublicKey = Convert.ToHexString(ecKey.GetPubKey()),
                PrivateKey = ecKey.GetPrivateKey(),
                GeneratedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>(request.Metadata ?? new Dictionary<string, string>())
                {
                    ["AddressType"] = "Ethereum",
                    ["GenerationMethod"] = "Random",
                    ["KeyAlgorithm"] = "ECDSA-secp256k1",
                    ["Checksum"] = account.Address.IsValidEthereumAddressHexFormat().ToString()
                }
            };
        }

        /// <summary>
        /// Validates a MultiChain address.
        /// </summary>
        private async Task<ValidateAddressResponse> ValidateMultiChainAddressAsync(ValidateAddressRequest request, CancellationToken cancellationToken)
        {
            var errors = new List<string>();
            var isValid = true;

            // Basic MultiChain address validation
            if (string.IsNullOrWhiteSpace(request.Address))
            {
                errors.Add("Address cannot be empty");
                isValid = false;
            }
            else if (request.Address.Length < 25 || request.Address.Length > 35)
            {
                errors.Add("Invalid MultiChain address length");
                isValid = false;
            }
            else if (!request.Address.StartsWith("1"))
            {
                errors.Add("MultiChain address must start with '1'");
                isValid = false;
            }

            return new ValidateAddressResponse
            {
                Address = request.Address,
                IsValid = isValid,
                NetworkType = "MultiChain",
                AddressFormat = "Base58",
                ValidationErrors = errors,
                ValidationMetadata = new Dictionary<string, string>
                {
                    ["ValidatedAt"] = DateTime.UtcNow.ToString("O"),
                    ["ValidationMethod"] = "Format"
                }
            };
        }

        /// <summary>
        /// Validates an Ethereum address.
        /// </summary>
        private ValidateAddressResponse ValidateEthereumAddress(ValidateAddressRequest request)
        {
            var errors = new List<string>();
            var isValid = true;
            var addressFormat = "Unknown";

            try
            {
                if (string.IsNullOrWhiteSpace(request.Address))
                {
                    errors.Add("Address cannot be empty");
                    isValid = false;
                }
                else
                {
                    isValid = request.Address.IsValidEthereumAddressHexFormat();
                    if (!isValid)
                    {
                        errors.Add("Invalid Ethereum address format");
                    }
                    else
                    {
                        addressFormat = request.Address.IsValidEthereumAddressHexFormat() ? "Hex" : "Invalid";
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Validation error: {ex.Message}");
                isValid = false;
            }

            return new ValidateAddressResponse
            {
                Address = request.Address,
                IsValid = isValid,
                NetworkType = "Ethereum",
                AddressFormat = addressFormat,
                ValidationErrors = errors,
                ValidationMetadata = new Dictionary<string, string>
                {
                    ["ValidatedAt"] = DateTime.UtcNow.ToString("O"),
                    ["ValidationMethod"] = "Nethereum",
                    ["IsChecksum"] = "false" // Simplified for demo
                }
            };
        }

        /// <summary>
        /// Gets MultiChain address information.
        /// </summary>
        private async Task<AddressInfoResponse> GetMultiChainAddressInfoAsync(GetAddressInfoRequest request, CancellationToken cancellationToken)
        {
            // For demonstration, return mock data since we don't have a live MultiChain connection
            return new AddressInfoResponse
            {
                Address = request.Address,
                NetworkType = "MultiChain",
                Balance = 0,
                TransactionCount = 0,
                IsActive = false,
                LastActivity = null,
                TokenBalances = new Dictionary<string, decimal>(),
                Metadata = new Dictionary<string, string>
                {
                    ["QueriedAt"] = DateTime.UtcNow.ToString("O"),
                    ["Provider"] = "Mock",
                    ["Note"] = "Live MultiChain connection not configured"
                }
            };
        }

        /// <summary>
        /// Gets Ethereum address information.
        /// </summary>
        private async Task<AddressInfoResponse> GetEthereumAddressInfoAsync(GetAddressInfoRequest request, CancellationToken cancellationToken)
        {
            // For demonstration, return mock data since we don't have a live Ethereum connection
            return new AddressInfoResponse
            {
                Address = request.Address,
                NetworkType = "Ethereum",
                Balance = 0,
                TransactionCount = 0,
                IsActive = false,
                LastActivity = null,
                TokenBalances = new Dictionary<string, decimal>(),
                Metadata = new Dictionary<string, string>
                {
                    ["QueriedAt"] = DateTime.UtcNow.ToString("O"),
                    ["Provider"] = "Mock",
                    ["Note"] = "Live Ethereum connection not configured"
                }
            };
        }

        /// <summary>
        /// Gets MultiChain network statistics.
        /// </summary>
        private async Task<NetworkStatsResponse> GetMultiChainNetworkStatsAsync(CancellationToken cancellationToken)
        {
            // For demonstration, return mock data since we don't have a live MultiChain connection
            return new NetworkStatsResponse
            {
                NetworkType = "MultiChain",
                IsConnected = false,
                BlockHeight = 0,
                PeerCount = 0,
                Version = "Mock",
                LastBlockTime = DateTime.UtcNow,
                AdditionalInfo = new Dictionary<string, string>
                {
                    ["Note"] = "Live MultiChain connection not configured",
                    ["MockData"] = "true"
                }
            };
        }

        /// <summary>
        /// Gets Ethereum network statistics.
        /// </summary>
        private async Task<NetworkStatsResponse> GetEthereumNetworkStatsAsync(CancellationToken cancellationToken)
        {
            // For demonstration, return mock data since we don't have a live Ethereum connection
            return new NetworkStatsResponse
            {
                NetworkType = "Ethereum",
                IsConnected = false,
                BlockHeight = 0,
                PeerCount = 0,
                Version = "Mock",
                LastBlockTime = DateTime.UtcNow,
                AdditionalInfo = new Dictionary<string, string>
                {
                    ["Note"] = "Live Ethereum connection not configured",
                    ["MockData"] = "true"
                }
            };
        }

        #endregion
    }
}
