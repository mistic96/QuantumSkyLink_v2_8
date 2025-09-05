using System;
using System.Threading.Tasks;

namespace QuantumLedger.Cryptography.Interfaces
{
    /// <summary>
    /// Interface for multi-cloud key vault providers supporting hybrid encryption.
    /// Implements cost-optimized key management with single master key per provider.
    /// </summary>
    public interface IKeyVaultProvider
    {
        /// <summary>
        /// Gets the name of the key vault provider.
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Gets the monthly cost per 1M accounts for this provider.
        /// </summary>
        decimal MonthlyCostPer1MAccounts { get; }

        /// <summary>
        /// Gets the priority order for cost optimization (lower is better).
        /// </summary>
        int CostPriority { get; }

        /// <summary>
        /// Gets whether this provider is currently available and healthy.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Derives an encryption key for a specific account and algorithm using the master key.
        /// This implements the hybrid encryption approach for cost optimization.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="algorithm">The cryptographic algorithm.</param>
        /// <returns>The derived encryption key.</returns>
        Task<byte[]> DeriveEncryptionKeyAsync(string accountId, string algorithm);

        /// <summary>
        /// Encrypts data using the provider's master key and account-specific derivation.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="accountId">The account identifier for key derivation.</param>
        /// <param name="algorithm">The cryptographic algorithm.</param>
        /// <returns>The encrypted data.</returns>
        Task<byte[]> EncryptAsync(byte[] data, string accountId, string algorithm);

        /// <summary>
        /// Decrypts data using the provider's master key and account-specific derivation.
        /// </summary>
        /// <param name="encryptedData">The encrypted data.</param>
        /// <param name="accountId">The account identifier for key derivation.</param>
        /// <param name="algorithm">The cryptographic algorithm.</param>
        /// <returns>The decrypted data.</returns>
        Task<byte[]> DecryptAsync(byte[] encryptedData, string accountId, string algorithm);

        /// <summary>
        /// Validates the connection to the key vault provider.
        /// </summary>
        /// <returns>True if the connection is valid; otherwise, false.</returns>
        Task<bool> ValidateConnectionAsync();

        /// <summary>
        /// Gets the health status of the key vault provider.
        /// </summary>
        /// <returns>The health status information.</returns>
        Task<KeyVaultHealthStatus> GetHealthStatusAsync();

        /// <summary>
        /// Rotates the master key for this provider (advanced operation).
        /// </summary>
        /// <returns>True if the rotation was successful; otherwise, false.</returns>
        Task<bool> RotateMasterKeyAsync();

        /// <summary>
        /// Gets usage statistics for cost monitoring.
        /// </summary>
        /// <returns>The usage statistics.</returns>
        Task<KeyVaultUsageStats> GetUsageStatsAsync();
    }

    /// <summary>
    /// Represents the health status of a key vault provider.
    /// </summary>
    public class KeyVaultHealthStatus
    {
        /// <summary>
        /// Gets or sets whether the provider is healthy.
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// Gets or sets the response time in milliseconds.
        /// </summary>
        public double ResponseTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the last check timestamp.
        /// </summary>
        public DateTime LastChecked { get; set; }

        /// <summary>
        /// Gets or sets any error message if unhealthy.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets additional health details.
        /// </summary>
        public Dictionary<string, object>? Details { get; set; }
    }

    /// <summary>
    /// Represents usage statistics for a key vault provider.
    /// </summary>
    public class KeyVaultUsageStats
    {
        /// <summary>
        /// Gets or sets the number of encryption operations.
        /// </summary>
        public long EncryptionOperations { get; set; }

        /// <summary>
        /// Gets or sets the number of decryption operations.
        /// </summary>
        public long DecryptionOperations { get; set; }

        /// <summary>
        /// Gets or sets the number of key derivation operations.
        /// </summary>
        public long KeyDerivationOperations { get; set; }

        /// <summary>
        /// Gets or sets the estimated monthly cost based on usage.
        /// </summary>
        public decimal EstimatedMonthlyCost { get; set; }

        /// <summary>
        /// Gets or sets the period start for these statistics.
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Gets or sets the period end for these statistics.
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Gets or sets additional usage details.
        /// </summary>
        public Dictionary<string, object>? Details { get; set; }
    }

    /// <summary>
    /// Exception thrown when key vault operations fail.
    /// </summary>
    public class KeyVaultException : Exception
    {
        /// <summary>
        /// Gets the provider name where the error occurred.
        /// </summary>
        public string ProviderName { get; }

        /// <summary>
        /// Gets the operation that failed.
        /// </summary>
        public string Operation { get; }

        /// <summary>
        /// Initializes a new instance of the KeyVaultException class.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="operation">The operation that failed.</param>
        /// <param name="message">The error message.</param>
        public KeyVaultException(string providerName, string operation, string message)
            : base($"Key vault operation '{operation}' failed for provider '{providerName}': {message}")
        {
            ProviderName = providerName;
            Operation = operation;
        }

        /// <summary>
        /// Initializes a new instance of the KeyVaultException class.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="operation">The operation that failed.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public KeyVaultException(string providerName, string operation, string message, Exception innerException)
            : base($"Key vault operation '{operation}' failed for provider '{providerName}': {message}", innerException)
        {
            ProviderName = providerName;
            Operation = operation;
        }
    }
}
