using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumLedger.Models.Account
{
    /// <summary>
    /// Represents a cryptographic key associated with an account in the multi-cloud system.
    /// Supports multiple algorithms (Dilithium, Falcon, EC256) with cloud provider storage.
    /// </summary>
    public class AccountKey
    {
        /// <summary>
        /// Gets or sets the unique key identifier.
        /// </summary>
        [Key]
        public Guid KeyId { get; set; }

        /// <summary>
        /// Gets or sets the account identifier this key belongs to.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Account))]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the cryptographic algorithm used for this key.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the public key in base64 format.
        /// </summary>
        [Required]
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the cloud provider where the private key is stored.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string CloudProvider { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the storage path/identifier for the private key in the cloud provider.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string StoragePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the key was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the current status of the key.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Gets or sets when the key was last rotated.
        /// </summary>
        public DateTime? LastRotated { get; set; }

        /// <summary>
        /// Gets or sets when the key expires (optional).
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Navigation property to the associated account.
        /// </summary>
        public virtual Account Account { get; set; } = null!;

        /// <summary>
        /// Validates that the account key has all required fields and valid data.
        /// </summary>
        /// <returns>True if the account key is valid; otherwise, false.</returns>
        public bool IsValid()
        {
            if (KeyId == Guid.Empty) return false;
            if (AccountId == Guid.Empty) return false;
            if (string.IsNullOrWhiteSpace(Algorithm)) return false;
            if (string.IsNullOrWhiteSpace(PublicKey)) return false;
            if (string.IsNullOrWhiteSpace(CloudProvider)) return false;
            if (string.IsNullOrWhiteSpace(StoragePath)) return false;
            if (string.IsNullOrWhiteSpace(Status)) return false;
            if (CreatedAt == default) return false;

            // Validate algorithm
            if (!IsValidAlgorithm(Algorithm)) return false;

            // Validate cloud provider
            if (!IsValidCloudProvider(CloudProvider)) return false;

            return true;
        }

        /// <summary>
        /// Checks if the algorithm is supported.
        /// </summary>
        /// <param name="algorithm">The algorithm to validate.</param>
        /// <returns>True if the algorithm is supported; otherwise, false.</returns>
        public static bool IsValidAlgorithm(string algorithm)
        {
            return algorithm switch
            {
                "Dilithium" => true,
                "Falcon" => true,
                "EC256" => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the cloud provider is supported.
        /// </summary>
        /// <param name="provider">The cloud provider to validate.</param>
        /// <returns>True if the cloud provider is supported; otherwise, false.</returns>
        public static bool IsValidCloudProvider(string provider)
        {
            return provider switch
            {
                "AWS" => true,
                "Azure" => true,
                "GoogleCloud" => true,
                _ => false
            };
        }

        /// <summary>
        /// Marks the key as rotated with the current timestamp.
        /// </summary>
        public void MarkAsRotated()
        {
            LastRotated = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the key is expired.
        /// </summary>
        /// <returns>True if the key is expired; otherwise, false.</returns>
        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the key is active and not expired.
        /// </summary>
        /// <returns>True if the key is active and not expired; otherwise, false.</returns>
        public bool IsActive()
        {
            return Status == "Active" && !IsExpired();
        }
    }

    /// <summary>
    /// Supported cryptographic algorithms for account keys.
    /// </summary>
    public static class SupportedAlgorithms
    {
        public const string Dilithium = "Dilithium";
        public const string Falcon = "Falcon";
        public const string EC256 = "EC256";

        public static readonly string[] All = { Dilithium, Falcon, EC256 };
    }

    /// <summary>
    /// Supported cloud providers for key storage.
    /// </summary>
    public static class SupportedCloudProviders
    {
        public const string AWS = "AWS";
        public const string Azure = "Azure";
        public const string GoogleCloud = "GoogleCloud";

        public static readonly string[] All = { AWS, Azure, GoogleCloud };
    }
}
