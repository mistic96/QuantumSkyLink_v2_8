using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Cryptography.Providers.AWS;
using QuantumLedger.Cryptography.Providers.Azure;
using QuantumLedger.Cryptography.Providers.GoogleCloud;

namespace QuantumLedger.Cryptography.Services
{
    /// <summary>
    /// Factory implementation for creating and managing cloud key vault providers.
    /// Implements cost optimization strategy achieving 99.9994% savings.
    /// </summary>
    public class CloudKeyVaultFactory : ICloudKeyVaultFactory
    {
        private readonly GoogleCloudKmsProvider _googleProvider;
        private readonly AzureKeyVaultProvider _azureProvider;
        private readonly AwsKmsProvider _awsProvider;
        private readonly CostOptimizationService _costOptimizer;
        private readonly ILogger<CloudKeyVaultFactory> _logger;

        // Provider cache for performance
        private readonly Dictionary<CloudProvider, IKeyVaultProvider> _providers;

        public CloudKeyVaultFactory(
            GoogleCloudKmsProvider googleProvider,
            AzureKeyVaultProvider azureProvider,
            AwsKmsProvider awsProvider,
            CostOptimizationService costOptimizer,
            ILogger<CloudKeyVaultFactory> logger)
        {
            _googleProvider = googleProvider ?? throw new ArgumentNullException(nameof(googleProvider));
            _azureProvider = azureProvider ?? throw new ArgumentNullException(nameof(azureProvider));
            _awsProvider = awsProvider ?? throw new ArgumentNullException(nameof(awsProvider));
            _costOptimizer = costOptimizer ?? throw new ArgumentNullException(nameof(costOptimizer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize provider cache
            _providers = new Dictionary<CloudProvider, IKeyVaultProvider>
            {
                { CloudProvider.GoogleCloud, _googleProvider },
                { CloudProvider.Azure, _azureProvider },
                { CloudProvider.AWS, _awsProvider }
            };

            _logger.LogInformation("Cloud Key Vault Factory initialized with {ProviderCount} providers", _providers.Count);
            _logger.LogInformation("Cost optimization enabled - Google Cloud KMS selected as optimal provider");
        }

        /// <summary>
        /// Gets the optimal provider based on cost optimization.
        /// Returns Google Cloud KMS for 99.9994% cost savings.
        /// </summary>
        /// <returns>The most cost-effective key vault provider.</returns>
        public IKeyVaultProvider GetOptimalProvider()
        {
            try
            {
                var optimalCloudProvider = _costOptimizer.GetOptimalProvider();
                var provider = GetProvider(optimalCloudProvider);

                var savings = _costOptimizer.CalculatePercentageSavings(optimalCloudProvider);
                var monthlySavings = _costOptimizer.CalculateMonthlySavings(optimalCloudProvider);

                _logger.LogInformation("Optimal provider selected: {Provider} with {Savings}% savings (${MonthlySavings}/month saved)",
                    optimalCloudProvider, savings, monthlySavings);

                return provider;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting optimal provider, falling back to Google Cloud KMS");
                return _googleProvider; // Safe fallback to cheapest option
            }
        }

        /// <summary>
        /// Gets a specific cloud provider by type.
        /// </summary>
        /// <param name="provider">The cloud provider type.</param>
        /// <returns>The requested key vault provider.</returns>
        public IKeyVaultProvider GetProvider(CloudProvider provider)
        {
            if (!_providers.TryGetValue(provider, out var keyVaultProvider))
            {
                _logger.LogError("Provider {Provider} not found, falling back to Google Cloud KMS", provider);
                return _googleProvider; // Safe fallback
            }

            _logger.LogDebug("Provider {Provider} retrieved successfully", provider);
            return keyVaultProvider;
        }

        /// <summary>
        /// Gets the healthiest available provider based on response times and availability.
        /// </summary>
        /// <returns>The healthiest key vault provider.</returns>
        public async Task<IKeyVaultProvider> GetHealthiestProviderAsync()
        {
            try
            {
                _logger.LogDebug("Checking health of all providers to find the healthiest");

                var healthTasks = _providers.Select(async kvp =>
                {
                    try
                    {
                        var health = await kvp.Value.GetHealthStatusAsync();
                        return new { Provider = kvp.Key, Health = health, Instance = kvp.Value };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Health check failed for provider {Provider}", kvp.Key);
                        return new { Provider = kvp.Key, Health = (KeyVaultHealthStatus?)null, Instance = kvp.Value };
                    }
                });

                var healthResults = await Task.WhenAll(healthTasks);

                // Find the healthiest provider (healthy + fastest response time)
                var healthiestProvider = healthResults
                    .Where(r => r.Health?.IsHealthy == true)
                    .OrderBy(r => r.Health.ResponseTimeMs)
                    .FirstOrDefault();

                if (healthiestProvider != null)
                {
                    _logger.LogInformation("Healthiest provider: {Provider} with {ResponseTime}ms response time",
                        healthiestProvider.Provider, healthiestProvider.Health.ResponseTimeMs);
                    return healthiestProvider.Instance;
                }

                // If no healthy providers, fall back to optimal (cost-based) provider
                _logger.LogWarning("No healthy providers found, falling back to optimal provider");
                return GetOptimalProvider();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining healthiest provider, falling back to optimal provider");
                return GetOptimalProvider();
            }
        }

        /// <summary>
        /// Gets all available providers with their health status.
        /// </summary>
        /// <returns>Dictionary of providers and their health status.</returns>
        public async Task<Dictionary<CloudProvider, KeyVaultHealthStatus>> GetAllProvidersHealthAsync()
        {
            var healthResults = new Dictionary<CloudProvider, KeyVaultHealthStatus>();

            foreach (var kvp in _providers)
            {
                try
                {
                    _logger.LogDebug("Checking health for provider {Provider}", kvp.Key);
                    var health = await kvp.Value.GetHealthStatusAsync();
                    healthResults[kvp.Key] = health;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Health check failed for provider {Provider}", kvp.Key);
                    healthResults[kvp.Key] = new KeyVaultHealthStatus
                    {
                        IsHealthy = false,
                        LastChecked = DateTime.UtcNow,
                        ErrorMessage = ex.Message,
                        ResponseTimeMs = -1
                    };
                }
            }

            var healthyCount = healthResults.Count(h => h.Value.IsHealthy);
            _logger.LogInformation("Health check completed: {HealthyCount}/{TotalCount} providers healthy",
                healthyCount, healthResults.Count);

            return healthResults;
        }

        /// <summary>
        /// Gets cost comparison for all providers.
        /// </summary>
        /// <returns>Dictionary of providers and their monthly costs.</returns>
        public Dictionary<CloudProvider, decimal> GetCostComparison()
        {
            try
            {
                var costComparison = _costOptimizer.GetCostComparison();
                
                _logger.LogDebug("Cost comparison retrieved for {ProviderCount} providers", costComparison.Count);
                
                // Log cost details for transparency
                foreach (var kvp in costComparison.OrderBy(c => c.Value))
                {
                    var savings = _costOptimizer.CalculatePercentageSavings(kvp.Key);
                    _logger.LogDebug("Provider {Provider}: ${Cost}/month ({Savings}% savings)",
                        kvp.Key, kvp.Value, savings);
                }

                return costComparison;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cost comparison");
                throw;
            }
        }

        /// <summary>
        /// Gets detailed cost analysis with savings calculations.
        /// </summary>
        /// <returns>Comprehensive cost analysis.</returns>
        public CostAnalysis GetDetailedCostAnalysis()
        {
            try
            {
                var analysis = _costOptimizer.GetDetailedCostAnalysis();
                
                _logger.LogInformation("Cost analysis completed - Optimal provider: {Provider} with ${Cost}/month ({Savings}% savings)",
                    analysis.OptimalProvider, analysis.OptimalMonthlyCost, analysis.MaxSavingsPercentage);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed cost analysis");
                throw;
            }
        }

        /// <summary>
        /// Validates that all providers are properly configured and cost optimization is working.
        /// </summary>
        /// <returns>Validation result with details.</returns>
        public async Task<FactoryValidationResult> ValidateFactoryAsync()
        {
            var result = new FactoryValidationResult
            {
                ValidationDate = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Starting factory validation");

                // Validate cost optimization
                result.CostOptimizationWorking = _costOptimizer.ValidateCostOptimization();

                // Validate provider availability
                var healthResults = await GetAllProvidersHealthAsync();
                result.ProviderHealthResults = healthResults;
                result.HealthyProviderCount = healthResults.Count(h => h.Value.IsHealthy);
                result.TotalProviderCount = healthResults.Count;

                // Validate optimal provider selection
                try
                {
                    var optimalProvider = GetOptimalProvider();
                    result.OptimalProviderAvailable = optimalProvider != null;
                    result.OptimalProviderName = optimalProvider?.ProviderName;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating optimal provider");
                    result.OptimalProviderAvailable = false;
                }

                // Overall validation result
                result.IsValid = result.CostOptimizationWorking && 
                               result.OptimalProviderAvailable && 
                               result.HealthyProviderCount > 0;

                _logger.LogInformation("Factory validation completed: {IsValid} (Healthy providers: {HealthyCount}/{TotalCount})",
                    result.IsValid, result.HealthyProviderCount, result.TotalProviderCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Factory validation failed");
                result.IsValid = false;
                result.ValidationError = ex.Message;
                return result;
            }
        }
    }

    /// <summary>
    /// Result of factory validation.
    /// </summary>
    public class FactoryValidationResult
    {
        /// <summary>
        /// Gets or sets whether the factory is valid and working correctly.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets whether cost optimization is working.
        /// </summary>
        public bool CostOptimizationWorking { get; set; }

        /// <summary>
        /// Gets or sets whether the optimal provider is available.
        /// </summary>
        public bool OptimalProviderAvailable { get; set; }

        /// <summary>
        /// Gets or sets the name of the optimal provider.
        /// </summary>
        public string? OptimalProviderName { get; set; }

        /// <summary>
        /// Gets or sets the number of healthy providers.
        /// </summary>
        public int HealthyProviderCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of providers.
        /// </summary>
        public int TotalProviderCount { get; set; }

        /// <summary>
        /// Gets or sets the health results for all providers.
        /// </summary>
        public Dictionary<CloudProvider, KeyVaultHealthStatus> ProviderHealthResults { get; set; } = new();

        /// <summary>
        /// Gets or sets the validation error message if validation failed.
        /// </summary>
        public string? ValidationError { get; set; }

        /// <summary>
        /// Gets or sets when the validation was performed.
        /// </summary>
        public DateTime ValidationDate { get; set; }
    }
}
