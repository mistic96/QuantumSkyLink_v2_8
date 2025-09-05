using System.ComponentModel.DataAnnotations;
using QuantumLedger.Hub.Models;

namespace QuantumLedger.Hub.Models
{
    /// <summary>
    /// Request model for creating/submitting transactions.
    /// </summary>
    public class CreateTransactionRequest
    {
        [Required]
        public Guid AccountId { get; set; }
        
        [Required]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        public string? Currency { get; set; }
        
        public Guid? ToAccountId { get; set; }
        
        public string? Description { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Response model for transaction details.
    /// </summary>
    public class TransactionResponse
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
        public long? BlockNumber { get; set; }
        public decimal TransactionFee { get; set; }
        public long? GasUsed { get; set; }
        public long? GasPrice { get; set; }
        public int? Confirmations { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Request model for transaction validation.
    /// </summary>
    public class ValidateTransactionRequest
    {
        [Required]
        public Guid AccountId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        public string? Currency { get; set; }
        
        public string? Type { get; set; }
        
        public Guid? ToAccountId { get; set; }
    }

    /// <summary>
    /// Response model for transaction validation.
    /// </summary>
    public class ValidationResponse
    {
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public decimal EstimatedFee { get; set; }
        public long EstimatedGas { get; set; }
        public TimeSpan EstimatedProcessingTime { get; set; }
        public DateTime ValidatedAt { get; set; }
    }

    /// <summary>
    /// Response model for transaction cancellation.
    /// </summary>
    public class CancelTransactionResponse
    {
        public Guid TransactionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CancelledAt { get; set; }
        public string? Reason { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal RefundFee { get; set; }
    }

    /// <summary>
    /// Response model for ledger statistics.
    /// </summary>
    public class LedgerStatsResponse
    {
        public int PeriodDays { get; set; }
        public long TotalTransactions { get; set; }
        public decimal TotalVolume { get; set; }
        public decimal TotalFees { get; set; }
        public long ActiveAccounts { get; set; }
        public decimal AverageTransactionSize { get; set; }
        public Dictionary<string, long> TransactionsByType { get; set; } = new();
        public Dictionary<string, decimal> VolumeByType { get; set; } = new();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    /// <summary>
    /// Legacy request model for compatibility with existing CQRS handlers.
    /// </summary>
    public class LegacyTransactionRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        public string? Type { get; set; }
        
        public decimal Amount { get; set; }
        
        public string? FromAddress { get; set; }
        
        public string? ToAddress { get; set; }
        
        public Dictionary<string, object>? Data { get; set; }
    }
}
