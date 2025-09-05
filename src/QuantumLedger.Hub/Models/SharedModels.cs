using System.ComponentModel.DataAnnotations;

namespace QuantumLedger.Hub.Models
{
    /// <summary>
    /// Standard error response model used across all endpoints.
    /// </summary>
    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? ErrorCode { get; set; }
    }

    /// <summary>
    /// Base response model for paginated results.
    /// </summary>
    public class PaginatedResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Currency balance information.
    /// </summary>
    public class CurrencyBalance
    {
        public string Currency { get; set; } = string.Empty;
        public decimal Available { get; set; }
        public decimal Pending { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Key information for cryptographic operations.
    /// </summary>
    public class KeyInfo
    {
        public string Algorithm { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string CloudProvider { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Active";
        public int UsageCount { get; set; }
        public DateTime? LastUsed { get; set; }
    }

    /// <summary>
    /// Transaction summary for lists and history.
    /// </summary>
    public class TransactionSummary
    {
        public Guid TransactionId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? FromAccountId { get; set; }
        public Guid? ToAccountId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? BlockHash { get; set; }
        public decimal TransactionFee { get; set; }
    }

    /// <summary>
    /// Account summary for lists and references.
    /// </summary>
    public class AccountSummary
    {
        public Guid AccountId { get; set; }
        public string ExternalOwnerId { get; set; } = string.Empty;
        public string VendorSystem { get; set; } = string.Empty;
        public string OwnerType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivity { get; set; }
    }
}
