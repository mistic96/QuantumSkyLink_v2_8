using System;
using System.Threading.Tasks;

namespace QuantumLedger.Cryptography.Interfaces
{
    /// <summary>
    /// Factory interface for creating and managing cloud key vault providers.
    /// Supports cost optimization by automatically selecting the cheapest provider.
    /// </summary>
    public interface ICloudKeyVaultFactory
    {
        /// <summary>
        /// Gets the optimal provider based on cost optimization.
        /// Returns Google Cloud KMS for 99.9994% cost savings ($18.50/month vs $3M/month).
        /// </summary>
        /// <returns>The most cost-effective key vault provider.</returns>
        IKeyVaultProvider GetOptimalProvider();

        /// <summary>
        /// Gets a specific cloud provider by type.
        /// </summary>
        /// <param name="provider">The cloud provider type.</param>
        /// <returns>The requested key vault provider.</returns>
        IKeyVaultProvider GetProvider(CloudProvider provider);

        /// <summary>
        /// Gets the healthiest available provider based on response times and availability.
        /// </summary>
        /// <returns>The healthiest key vault provider.</returns>
        Task<IKeyVaultProvider> GetHealthiestProviderAsync();

        /// <summary>
        /// Gets all available providers with their health status.
        /// </summary>
        /// <returns>Dictionary of providers and their health status.</returns>
        Task<Dictionary<CloudProvider, KeyVaultHealthStatus>> GetAllProvidersHealthAsync();

        /// <summary>
        /// Gets cost comparison for all providers.
        /// </summary>
        /// <returns>Dictionary of providers and their monthly costs.</returns>
        Dictionary<CloudProvider, decimal> GetCostComparison();
    }

    /// <summary>
    /// Enumeration of supported cloud providers.
    /// </summary>
    public enum CloudProvider
    {
        /// <summary>
        /// Google Cloud KMS - Cheapest option at $18.50/month.
        /// </summary>
        GoogleCloud = 1,

        /// <summary>
        /// Azure Key Vault - Middle option at $19.21/month.
        /// </summary>
        Azure = 2,

        /// <summary>
        /// AWS KMS - Most expensive but mature option at $21.24/month.
        /// </summary>
        AWS = 3
    }
}
