using System;
using LiquidStorageCloud.Core.Database;

namespace QuantumLedger.Cryptography.Models
{
    /// <summary>
    /// Key categories supported by the system
    /// </summary>
    public enum KeyCategory
    {
        Traditional,
        PostQuantum,
        Substitution    // NEW - User-controlled delegation keys
    }

    /// <summary>
    /// Represents a cryptographic key entity in the system
    /// </summary>
    public class KeyEntity : ISurrealEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for this key
        /// </summary>
        public required string Id { get; set; }

        public bool SolidState { get; set; }
        public DateTimeOffset LastModified { get; set; }

        /// <summary>
        /// Gets the table name for SurrealDB
        /// </summary>
        public string TableName => "keys";

        /// <summary>
        /// Gets the namespace for SurrealDB
        /// </summary>
        public string Namespace => "crypto";

        /// <summary>
        /// Gets or sets the address this key belongs to
        /// </summary>
        public required string Address { get; set; }

        /// <summary>
        /// Gets or sets the key category (Traditional, PostQuantum, or Substitution)
        /// </summary>
        public required KeyCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the specific algorithm used (EC-256, DILITHIUM, FALCON, or EC-256 for substitution)
        /// </summary>
        public required string Algorithm { get; set; }

        /// <summary>
        /// Gets or sets the key identifier used in cryptographic operations
        /// </summary>
        public required string KeyIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the encrypted key data
        /// </summary>
        public required string EncryptedKeyData { get; set; }

        /// <summary>
        /// Gets or sets the reference to the encrypted backup in S3
        /// </summary>
        public required string S3Reference { get; set; }

        /// <summary>
        /// Gets or sets when this key was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when this key was revoked (if applicable)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Gets whether this key is currently active
        /// </summary>
        public bool IsActive => !RevokedAt.HasValue;

        /// <summary>
        /// Gets or sets the version of this key
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the previous version's key identifier (if any)
        /// </summary>
        public string? PreviousVersionId { get; set; }

        /// <summary>
        /// Gets or sets metadata about the key
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Gets or sets the last time this key was accessed
        /// </summary>
        public DateTime LastAccessedAt { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of this key (if any)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets whether this key has expired
        /// </summary>
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the KMS key ID used to encrypt this key
        /// </summary>
        public required string KmsKeyId { get; set; }

        /// <summary>
        /// Gets or sets the checksum of the encrypted key data
        /// </summary>
        public required string Checksum { get; set; }

        /// <summary>
        /// Gets whether this is a substitution key (user-controlled delegation key)
        /// </summary>
        public bool IsSubstitutionKey => Category == KeyCategory.Substitution;

        /// <summary>
        /// Gets or sets the linked main account address (for substitution keys only)
        /// </summary>
        public string? LinkedMainAddress { get; set; }
    }
}
