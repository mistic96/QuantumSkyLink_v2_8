using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuantumLedger.Cryptography.Interfaces
{
    /// <summary>
    /// Interface for multi-cloud storage providers for secure private key storage.
    /// Supports AWS S3, Azure Blob Storage, and Google Cloud Storage.
    /// </summary>
    public interface ICloudStorageProvider
    {
        /// <summary>
        /// Gets the name of the cloud storage provider.
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Gets the storage cost per GB per month for this provider.
        /// </summary>
        decimal StorageCostPerGBPerMonth { get; }

        /// <summary>
        /// Gets whether this provider is currently available and healthy.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Stores encrypted data in the cloud storage with the specified path.
        /// </summary>
        /// <param name="data">The encrypted data to store.</param>
        /// <param name="accountId">The account identifier for path generation.</param>
        /// <param name="algorithm">The cryptographic algorithm for path organization.</param>
        /// <param name="metadata">Optional metadata to store with the data.</param>
        /// <returns>The storage path/identifier for the stored data.</returns>
        Task<string> StoreAsync(byte[] data, string accountId, string algorithm, Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Retrieves encrypted data from the cloud storage using the specified path.
        /// </summary>
        /// <param name="path">The storage path/identifier.</param>
        /// <returns>The encrypted data.</returns>
        Task<byte[]> RetrieveAsync(string path);

        /// <summary>
        /// Retrieves encrypted data along with its metadata.
        /// </summary>
        /// <param name="path">The storage path/identifier.</param>
        /// <returns>The encrypted data and metadata.</returns>
        Task<StorageResult> RetrieveWithMetadataAsync(string path);

        /// <summary>
        /// Deletes data from the cloud storage.
        /// </summary>
        /// <param name="path">The storage path/identifier.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteAsync(string path);

        /// <summary>
        /// Checks if data exists at the specified path.
        /// </summary>
        /// <param name="path">The storage path/identifier.</param>
        /// <returns>True if the data exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(string path);

        /// <summary>
        /// Lists all storage paths for a specific account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns>A list of storage paths for the account.</returns>
        Task<IEnumerable<string>> ListAccountPathsAsync(string accountId);

        /// <summary>
        /// Gets metadata for stored data without retrieving the data itself.
        /// </summary>
        /// <param name="path">The storage path/identifier.</param>
        /// <returns>The metadata for the stored data.</returns>
        Task<Dictionary<string, string>?> GetMetadataAsync(string path);

        /// <summary>
        /// Updates metadata for stored data.
        /// </summary>
        /// <param name="path">The storage path/identifier.</param>
        /// <param name="metadata">The new metadata.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> UpdateMetadataAsync(string path, Dictionary<string, string> metadata);

        /// <summary>
        /// Validates the connection to the cloud storage provider.
        /// </summary>
        /// <returns>True if the connection is valid; otherwise, false.</returns>
        Task<bool> ValidateConnectionAsync();

        /// <summary>
        /// Gets the health status of the cloud storage provider.
        /// </summary>
        /// <returns>The health status information.</returns>
        Task<CloudStorageHealthStatus> GetHealthStatusAsync();

        /// <summary>
        /// Gets usage statistics for cost monitoring.
        /// </summary>
        /// <returns>The usage statistics.</returns>
        Task<CloudStorageUsageStats> GetUsageStatsAsync();

        /// <summary>
        /// Creates a backup of data to a different region or storage class.
        /// </summary>
        /// <param name="path">The storage path/identifier.</param>
        /// <param name="backupOptions">The backup configuration options.</param>
        /// <returns>The backup path/identifier.</returns>
        Task<string> CreateBackupAsync(string path, BackupOptions? backupOptions = null);

        /// <summary>
        /// Restores data from a backup.
        /// </summary>
        /// <param name="backupPath">The backup path/identifier.</param>
        /// <param name="restorePath">The path to restore to.</param>
        /// <returns>True if the restore was successful; otherwise, false.</returns>
        Task<bool> RestoreFromBackupAsync(string backupPath, string restorePath);
    }

    /// <summary>
    /// Represents the result of a storage retrieval operation with metadata.
    /// </summary>
    public class StorageResult
    {
        /// <summary>
        /// Gets or sets the retrieved data.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets or sets the metadata associated with the data.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Gets or sets when the data was last modified.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the size of the data in bytes.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the content type/MIME type of the data.
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the ETag or version identifier.
        /// </summary>
        public string? ETag { get; set; }
    }

    /// <summary>
    /// Represents backup configuration options.
    /// </summary>
    public class BackupOptions
    {
        /// <summary>
        /// Gets or sets the backup region (for cross-region backup).
        /// </summary>
        public string? BackupRegion { get; set; }

        /// <summary>
        /// Gets or sets the storage class for the backup.
        /// </summary>
        public string? StorageClass { get; set; }

        /// <summary>
        /// Gets or sets whether to encrypt the backup.
        /// </summary>
        public bool EncryptBackup { get; set; } = true;

        /// <summary>
        /// Gets or sets the retention period for the backup in days.
        /// </summary>
        public int RetentionDays { get; set; } = 30;

        /// <summary>
        /// Gets or sets additional backup metadata.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }

    /// <summary>
    /// Represents the health status of a cloud storage provider.
    /// </summary>
    public class CloudStorageHealthStatus
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
        /// Gets or sets the available storage capacity.
        /// </summary>
        public long? AvailableCapacityBytes { get; set; }

        /// <summary>
        /// Gets or sets the used storage capacity.
        /// </summary>
        public long? UsedCapacityBytes { get; set; }

        /// <summary>
        /// Gets or sets additional health details.
        /// </summary>
        public Dictionary<string, object>? Details { get; set; }
    }

    /// <summary>
    /// Represents usage statistics for a cloud storage provider.
    /// </summary>
    public class CloudStorageUsageStats
    {
        /// <summary>
        /// Gets or sets the number of storage operations.
        /// </summary>
        public long StorageOperations { get; set; }

        /// <summary>
        /// Gets or sets the number of retrieval operations.
        /// </summary>
        public long RetrievalOperations { get; set; }

        /// <summary>
        /// Gets or sets the number of deletion operations.
        /// </summary>
        public long DeletionOperations { get; set; }

        /// <summary>
        /// Gets or sets the total bytes stored.
        /// </summary>
        public long TotalBytesStored { get; set; }

        /// <summary>
        /// Gets or sets the total bytes transferred.
        /// </summary>
        public long TotalBytesTransferred { get; set; }

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
    /// Exception thrown when cloud storage operations fail.
    /// </summary>
    public class CloudStorageException : Exception
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
        /// Gets the storage path involved in the operation.
        /// </summary>
        public string? StoragePath { get; }

        /// <summary>
        /// Initializes a new instance of the CloudStorageException class.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="operation">The operation that failed.</param>
        /// <param name="message">The error message.</param>
        /// <param name="storagePath">The storage path involved.</param>
        public CloudStorageException(string providerName, string operation, string message, string? storagePath = null)
            : base($"Cloud storage operation '{operation}' failed for provider '{providerName}': {message}")
        {
            ProviderName = providerName;
            Operation = operation;
            StoragePath = storagePath;
        }

        /// <summary>
        /// Initializes a new instance of the CloudStorageException class.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="operation">The operation that failed.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="storagePath">The storage path involved.</param>
        public CloudStorageException(string providerName, string operation, string message, Exception innerException, string? storagePath = null)
            : base($"Cloud storage operation '{operation}' failed for provider '{providerName}': {message}", innerException)
        {
            ProviderName = providerName;
            Operation = operation;
            StoragePath = storagePath;
        }
    }
}
