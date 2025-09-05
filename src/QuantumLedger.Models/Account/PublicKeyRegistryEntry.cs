using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumLedger.Models.Account
{
    /// <summary>
    /// Represents an entry in the public key registry for high-performance signature verification.
    /// Optimized for sub-100ms lookup performance across 1M+ accounts.
    /// </summary>
    public class PublicKeyRegistryEntry
    {
        /// <summary>
        /// Gets or sets the SHA-256 hash of the public key (primary key for fast lookups).
        /// </summary>
        [Key]
        [MaxLength(64)]
        public string PublicKeyHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account identifier this public key belongs to.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Account))]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the cryptographic algorithm used for this public key.
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
        /// Gets or sets when the public key was registered.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the current status of the public key.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Gets or sets when the public key was last used for verification.
        /// </summary>
        public DateTime? LastUsed { get; set; }

        /// <summary>
        /// Gets or sets the number of times this public key has been used for verification.
        /// </summary>
        public long UsageCount { get; set; } = 0;

        /// <summary>
        /// Navigation property to the associated account.
        /// </summary>
        public virtual Account Account { get; set; } = null!;

        /// <summary>
        /// Validates that the public key registry entry has all required fields and valid data.
        /// </summary>
        /// <returns>True if the entry is valid; otherwise, false.</returns>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(PublicKeyHash)) return false;
            if (PublicKeyHash.Length != 64) return false; // SHA-256 hash length
            if (AccountId == Guid.Empty) return false;
            if (string.IsNullOrWhiteSpace(Algorithm)) return false;
            if (string.IsNullOrWhiteSpace(PublicKey)) return false;
            if (string.IsNullOrWhiteSpace(Status)) return false;
            if (CreatedAt == default) return false;

            // Validate algorithm
            if (!AccountKey.IsValidAlgorithm(Algorithm)) return false;

            return true;
        }

        /// <summary>
        /// Generates the SHA-256 hash of the public key for indexing.
        /// </summary>
        /// <param name="publicKey">The public key to hash.</param>
        /// <returns>The SHA-256 hash as a hexadecimal string.</returns>
        public static string GeneratePublicKeyHash(string publicKey)
        {
            if (string.IsNullOrWhiteSpace(publicKey))
                throw new ArgumentException("Public key cannot be null or empty", nameof(publicKey));

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(publicKey));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Updates the public key hash based on the current public key.
        /// </summary>
        public void UpdateHash()
        {
            PublicKeyHash = GeneratePublicKeyHash(PublicKey);
        }

        /// <summary>
        /// Records usage of this public key for verification.
        /// </summary>
        public void RecordUsage()
        {
            LastUsed = DateTime.UtcNow;
            UsageCount++;
        }

        /// <summary>
        /// Checks if the public key is active and can be used for verification.
        /// </summary>
        /// <returns>True if the public key is active; otherwise, false.</returns>
        public bool IsActive()
        {
            return Status == "Active";
        }

        /// <summary>
        /// Deactivates the public key entry.
        /// </summary>
        public void Deactivate()
        {
            Status = "Inactive";
        }

        /// <summary>
        /// Reactivates the public key entry.
        /// </summary>
        public void Activate()
        {
            Status = "Active";
        }
    }

    /// <summary>
    /// Extension methods for public key registry operations.
    /// </summary>
    public static class PublicKeyRegistryExtensions
    {
        /// <summary>
        /// Creates a new public key registry entry from an account key.
        /// </summary>
        /// <param name="accountKey">The account key to create the registry entry from.</param>
        /// <returns>A new public key registry entry.</returns>
        public static PublicKeyRegistryEntry ToRegistryEntry(this AccountKey accountKey)
        {
            var entry = new PublicKeyRegistryEntry
            {
                AccountId = accountKey.AccountId,
                Algorithm = accountKey.Algorithm,
                PublicKey = accountKey.PublicKey,
                CreatedAt = DateTime.UtcNow,
                Status = accountKey.Status
            };

            entry.UpdateHash();
            return entry;
        }

        /// <summary>
        /// Validates that a public key matches its hash.
        /// </summary>
        /// <param name="entry">The registry entry to validate.</param>
        /// <returns>True if the public key matches its hash; otherwise, false.</returns>
        public static bool ValidateHash(this PublicKeyRegistryEntry entry)
        {
            var expectedHash = PublicKeyRegistryEntry.GeneratePublicKeyHash(entry.PublicKey);
            return string.Equals(entry.PublicKeyHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
