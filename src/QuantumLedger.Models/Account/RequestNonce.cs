using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumLedger.Models.Account
{
    /// <summary>
    /// Represents a request nonce for replay attack prevention in signature verification.
    /// Implements time-based expiration with uniqueness validation.
    /// </summary>
    public class RequestNonce
    {
        /// <summary>
        /// Gets or sets the SHA-256 hash of the nonce (primary key for fast lookups).
        /// </summary>
        [Key]
        [MaxLength(64)]
        public string NonceHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account identifier this nonce belongs to.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Account))]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the original nonce value.
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string OriginalNonce { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the nonce was created/used.
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when the nonce expires and can be cleaned up.
        /// </summary>
        [Required]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the request type or operation this nonce was used for.
        /// </summary>
        [MaxLength(100)]
        public string? RequestType { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the request (for audit purposes).
        /// </summary>
        [MaxLength(45)] // IPv6 max length
        public string? IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the user agent of the request (for audit purposes).
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Navigation property to the associated account.
        /// </summary>
        public virtual Account Account { get; set; } = null!;

        /// <summary>
        /// Validates that the request nonce has all required fields and valid data.
        /// </summary>
        /// <returns>True if the nonce is valid; otherwise, false.</returns>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(NonceHash)) return false;
            if (NonceHash.Length != 64) return false; // SHA-256 hash length
            if (AccountId == Guid.Empty) return false;
            if (string.IsNullOrWhiteSpace(OriginalNonce)) return false;
            if (Timestamp == default) return false;
            if (ExpiresAt == default) return false;
            if (ExpiresAt <= Timestamp) return false; // Expiration must be after creation

            return true;
        }

        /// <summary>
        /// Generates the SHA-256 hash of the nonce for indexing.
        /// </summary>
        /// <param name="nonce">The nonce to hash.</param>
        /// <returns>The SHA-256 hash as a hexadecimal string.</returns>
        public static string GenerateNonceHash(string nonce)
        {
            if (string.IsNullOrWhiteSpace(nonce))
                throw new ArgumentException("Nonce cannot be null or empty", nameof(nonce));

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(nonce));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Updates the nonce hash based on the current original nonce.
        /// </summary>
        public void UpdateHash()
        {
            NonceHash = GenerateNonceHash(OriginalNonce);
        }

        /// <summary>
        /// Checks if the nonce is expired.
        /// </summary>
        /// <returns>True if the nonce is expired; otherwise, false.</returns>
        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpiresAt;
        }

        /// <summary>
        /// Checks if the nonce is still valid (not expired).
        /// </summary>
        /// <returns>True if the nonce is still valid; otherwise, false.</returns>
        public bool IsStillValid()
        {
            return !IsExpired();
        }

        /// <summary>
        /// Creates a new request nonce with the specified parameters.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="nonce">The original nonce value.</param>
        /// <param name="expirationMinutes">The expiration time in minutes (default: 15 minutes).</param>
        /// <param name="requestType">The optional request type.</param>
        /// <param name="ipAddress">The optional IP address.</param>
        /// <param name="userAgent">The optional user agent.</param>
        /// <returns>A new request nonce.</returns>
        public static RequestNonce Create(
            Guid accountId,
            string nonce,
            int expirationMinutes = 15,
            string? requestType = null,
            string? ipAddress = null,
            string? userAgent = null)
        {
            var requestNonce = new RequestNonce
            {
                AccountId = accountId,
                OriginalNonce = nonce,
                Timestamp = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                RequestType = requestType,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            requestNonce.UpdateHash();
            return requestNonce;
        }
    }

    /// <summary>
    /// Extension methods for request nonce operations.
    /// </summary>
    public static class RequestNonceExtensions
    {
        /// <summary>
        /// Validates that a nonce matches its hash.
        /// </summary>
        /// <param name="requestNonce">The request nonce to validate.</param>
        /// <returns>True if the nonce matches its hash; otherwise, false.</returns>
        public static bool ValidateHash(this RequestNonce requestNonce)
        {
            var expectedHash = RequestNonce.GenerateNonceHash(requestNonce.OriginalNonce);
            return string.Equals(requestNonce.NonceHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the remaining time until the nonce expires.
        /// </summary>
        /// <param name="requestNonce">The request nonce.</param>
        /// <returns>The remaining time until expiration, or TimeSpan.Zero if already expired.</returns>
        public static TimeSpan GetRemainingTime(this RequestNonce requestNonce)
        {
            var remaining = requestNonce.ExpiresAt - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        /// <summary>
        /// Gets the age of the nonce since it was created.
        /// </summary>
        /// <param name="requestNonce">The request nonce.</param>
        /// <returns>The age of the nonce.</returns>
        public static TimeSpan GetAge(this RequestNonce requestNonce)
        {
            return DateTime.UtcNow - requestNonce.Timestamp;
        }
    }

    /// <summary>
    /// Configuration constants for nonce management.
    /// </summary>
    public static class NonceConfiguration
    {
        /// <summary>
        /// Default expiration time for nonces in minutes.
        /// </summary>
        public const int DefaultExpirationMinutes = 15;

        /// <summary>
        /// Maximum expiration time for nonces in minutes.
        /// </summary>
        public const int MaxExpirationMinutes = 60;

        /// <summary>
        /// Minimum expiration time for nonces in minutes.
        /// </summary>
        public const int MinExpirationMinutes = 1;

        /// <summary>
        /// Cleanup interval for expired nonces in minutes.
        /// </summary>
        public const int CleanupIntervalMinutes = 30;
    }
}
