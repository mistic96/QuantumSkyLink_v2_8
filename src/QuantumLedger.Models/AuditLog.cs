using System;
using System.ComponentModel.DataAnnotations;

namespace QuantumLedger.Models
{
    /// <summary>
    /// Represents an audit log entry in the Quantum Ledger system.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Gets or sets the ID of the request associated with this audit log.
        /// </summary>
        [Required]
        public string RequestId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the audit log was created.
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the status of the request processing.
        /// </summary>
        [Required]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets additional information or details about the request processing.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Validates that the audit log has all required fields and valid data.
        /// </summary>
        /// <returns>True if the audit log is valid; otherwise, false.</returns>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(RequestId)) return false;
            if (Timestamp == default) return false;
            if (string.IsNullOrWhiteSpace(Status)) return false;
            
            return true;
        }

        /// <summary>
        /// Common status values for audit logs.
        /// </summary>
        public static class Statuses
        {
            public const string Received = "Received";
            public const string Validated = "Validated";
            public const string SignatureVerified = "SignatureVerified";
            public const string Persisted = "Persisted";
            public const string BlockchainSubmitted = "BlockchainSubmitted";
            public const string Failed = "Failed";
        }
    }
}
