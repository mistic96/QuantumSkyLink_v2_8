using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumLedger.Models
{
    /// <summary>
    /// Represents a security audit log entry for compliance and monitoring
    /// Designed for SOC2 compliance and security event tracking
    /// </summary>
    [Table("SecurityAuditLogs")]
    public class SecurityAuditLog
    {
        /// <summary>
        /// Gets or sets the unique identifier for this audit log entry.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the timestamp when the event occurred.
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the type of security event.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the category of the event (Security, Authentication, DataAccess, etc.).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a human-readable description of the event.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets additional details about the event in JSON format.
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Gets or sets the user ID associated with the event.
        /// </summary>
        [MaxLength(100)]
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the IP address from which the event originated.
        /// </summary>
        [MaxLength(45)] // IPv6 max length
        public string? IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the user agent string from the request.
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Gets or sets the request ID for correlation with application logs.
        /// </summary>
        [MaxLength(100)]
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets or sets the severity level of the event.
        /// </summary>
        [MaxLength(20)]
        public string Severity { get; set; } = "Information";

        /// <summary>
        /// Gets or sets whether this event requires immediate attention.
        /// </summary>
        public bool RequiresAttention { get; set; } = false;

        /// <summary>
        /// Gets or sets when this audit log entry was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Validates that the security audit log has all required fields and valid data.
        /// </summary>
        /// <returns>True if the audit log is valid; otherwise, false.</returns>
        public bool IsValid()
        {
            if (Id == Guid.Empty) return false;
            if (Timestamp == default) return false;
            if (string.IsNullOrWhiteSpace(EventType)) return false;
            if (string.IsNullOrWhiteSpace(Category)) return false;
            if (string.IsNullOrWhiteSpace(Description)) return false;
            
            return true;
        }

        /// <summary>
        /// Sets the severity level based on the event type.
        /// </summary>
        public void SetSeverityFromEventType()
        {
            Severity = EventType.ToLowerInvariant() switch
            {
                var type when type.Contains("failed") || type.Contains("blocked") || type.Contains("unauthorized") => "Warning",
                var type when type.Contains("breach") || type.Contains("suspicious") || type.Contains("escalation") => "Critical",
                var type when type.Contains("error") || type.Contains("exception") => "Error",
                _ => "Information"
            };

            RequiresAttention = Severity is "Warning" or "Critical" or "Error";
        }

        /// <summary>
        /// Common event categories for security audit logs.
        /// </summary>
        public static class Categories
        {
            public const string Security = "Security";
            public const string Authentication = "Authentication";
            public const string Authorization = "Authorization";
            public const string DataAccess = "DataAccess";
            public const string DelegationKeySystem = "DelegationKeySystem";
            public const string SystemAccess = "SystemAccess";
            public const string Configuration = "Configuration";
            public const string Compliance = "Compliance";
        }

        /// <summary>
        /// Common severity levels for security audit logs.
        /// </summary>
        public static class SeverityLevels
        {
            public const string Information = "Information";
            public const string Warning = "Warning";
            public const string Error = "Error";
            public const string Critical = "Critical";
        }
    }

    /// <summary>
    /// Extension methods for SecurityAuditLog operations.
    /// </summary>
    public static class SecurityAuditLogExtensions
    {
        /// <summary>
        /// Creates a security audit log entry for authentication events.
        /// </summary>
        public static SecurityAuditLog CreateAuthenticationEvent(string eventType, string? userId = null, string? ipAddress = null, object? details = null)
        {
            var auditLog = new SecurityAuditLog
            {
                EventType = $"Authentication.{eventType}",
                Category = SecurityAuditLog.Categories.Authentication,
                Description = $"Authentication {eventType.ToLower()}",
                UserId = userId,
                IpAddress = ipAddress,
                Details = details != null ? System.Text.Json.JsonSerializer.Serialize(details) : null
            };

            auditLog.SetSeverityFromEventType();
            return auditLog;
        }

        /// <summary>
        /// Creates a security audit log entry for substitution key events.
        /// </summary>
        public static SecurityAuditLog CreateSubstitutionKeyEvent(string eventType, string keyId, string accountId, object? details = null, string? userId = null)
        {
            var eventDetails = new
            {
                KeyId = keyId,
                AccountId = accountId,
                AdditionalDetails = details
            };

            var auditLog = new SecurityAuditLog
            {
                EventType = $"SubstitutionKey.{eventType}",
                Category = SecurityAuditLog.Categories.DelegationKeySystem,
                Description = $"Substitution key {eventType.ToLower()}: {keyId}",
                UserId = userId,
                Details = System.Text.Json.JsonSerializer.Serialize(eventDetails)
            };

            auditLog.SetSeverityFromEventType();
            return auditLog;
        }

        /// <summary>
        /// Creates a security audit log entry for data access events.
        /// </summary>
        public static SecurityAuditLog CreateDataAccessEvent(string eventType, string resourceType, string resourceId, object? details = null, string? userId = null)
        {
            var eventDetails = new
            {
                ResourceType = resourceType,
                ResourceId = resourceId,
                AdditionalDetails = details
            };

            var auditLog = new SecurityAuditLog
            {
                EventType = $"DataAccess.{eventType}",
                Category = SecurityAuditLog.Categories.DataAccess,
                Description = $"Data {eventType.ToLower()}: {resourceType}/{resourceId}",
                UserId = userId,
                Details = System.Text.Json.JsonSerializer.Serialize(eventDetails)
            };

            auditLog.SetSeverityFromEventType();
            return auditLog;
        }

        /// <summary>
        /// Checks if the audit log represents a critical security event.
        /// </summary>
        public static bool IsCriticalEvent(this SecurityAuditLog auditLog)
        {
            return auditLog.Severity == SecurityAuditLog.SeverityLevels.Critical ||
                   auditLog.RequiresAttention;
        }

        /// <summary>
        /// Gets a summary of the audit log for reporting.
        /// </summary>
        public static string GetSummary(this SecurityAuditLog auditLog)
        {
            return $"[{auditLog.Timestamp:yyyy-MM-dd HH:mm:ss}] {auditLog.Severity}: {auditLog.EventType} - {auditLog.Description}";
        }
    }
}
