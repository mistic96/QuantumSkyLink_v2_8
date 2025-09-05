using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumLedger.Models.Account
{
    /// <summary>
    /// Represents an account in the multi-cloud account management system.
    /// Supports flexible external owner ID mapping for multi-vendor scenarios.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Gets or sets the unique account identifier.
        /// </summary>
        [Key]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the external owner identifier.
        /// This can be a Logto ID, email address, or custom vendor format.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string ExternalOwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of external owner ID.
        /// Examples: "LogtoId", "Email", "CustomId", etc.
        /// </summary>
        [MaxLength(50)]
        public string? OwnerIdType { get; set; }

        /// <summary>
        /// Gets or sets the vendor system that owns this account.
        /// Examples: "Internal", "VendorA", "VendorB", etc.
        /// </summary>
        [MaxLength(100)]
        public string? VendorSystem { get; set; }

        /// <summary>
        /// Gets or sets the optional internal reference ID (ULID format).
        /// Used for complex mapping scenarios between vendor systems.
        /// </summary>
        [MaxLength(26)]
        public string? InternalReferenceId { get; set; }

        /// <summary>
        /// Gets or sets the type of account owner.
        /// </summary>
        [Required]
        public OwnerType OwnerType { get; set; }

        /// <summary>
        /// Gets or sets when the account was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the current status of the account.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Gets or sets when the account was last updated.
        /// </summary>
        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// Navigation property for account keys.
        /// </summary>
        public virtual ICollection<AccountKey> AccountKeys { get; set; } = new List<AccountKey>();

        /// <summary>
        /// Navigation property for public key registry entries.
        /// </summary>
        public virtual ICollection<PublicKeyRegistryEntry> PublicKeyEntries { get; set; } = new List<PublicKeyRegistryEntry>();

        /// <summary>
        /// Navigation property for request nonces.
        /// </summary>
        public virtual ICollection<RequestNonce> RequestNonces { get; set; } = new List<RequestNonce>();

        /// <summary>
        /// Validates that the account has all required fields and valid data.
        /// </summary>
        /// <returns>True if the account is valid; otherwise, false.</returns>
        public bool IsValid()
        {
            if (AccountId == Guid.Empty) return false;
            if (string.IsNullOrWhiteSpace(ExternalOwnerId)) return false;
            if (ExternalOwnerId.Length > 500) return false;
            if (!Enum.IsDefined(typeof(OwnerType), OwnerType)) return false;
            if (string.IsNullOrWhiteSpace(Status)) return false;
            if (CreatedAt == default) return false;

            return true;
        }

        /// <summary>
        /// Updates the account's last updated timestamp.
        /// </summary>
        public void Touch()
        {
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Represents the type of account owner.
    /// </summary>
    public enum OwnerType
    {
        /// <summary>
        /// Client account for end users.
        /// </summary>
        Client = 1,

        /// <summary>
        /// System account for internal operations.
        /// </summary>
        System = 2
    }
}
