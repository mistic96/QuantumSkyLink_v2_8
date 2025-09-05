using System.ComponentModel.DataAnnotations;
using QuantumLedger.Hub.Models;

namespace QuantumLedger.Hub.Models
{
    /// <summary>
    /// Response model for account balance queries.
    /// </summary>
    public class BalanceResponse
    {
        public Guid AccountId { get; set; }
        public List<CurrencyBalance> Balances { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public decimal TotalValueUSD { get; set; }
    }

    /// <summary>
    /// Response model for transaction history with pagination.
    /// </summary>
    public class TransactionHistoryResponse : PaginatedResponse
    {
        public Guid AccountId { get; set; }
        public List<TransactionSummary> Transactions { get; set; } = new();
        public decimal TotalVolume { get; set; }
        public decimal TotalFees { get; set; }
    }

    /// <summary>
    /// Response model for pending transactions.
    /// </summary>
    public class PendingTransactionsResponse
    {
        public Guid AccountId { get; set; }
        public List<TransactionSummary> PendingTransactions { get; set; } = new();
        public int TotalPending { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public DateTime OldestPendingAt { get; set; }
    }

    /// <summary>
    /// Request model for balance adjustments (admin operations).
    /// </summary>
    public class BalanceAdjustmentRequest
    {
        [Required]
        public Guid AccountId { get; set; }
        
        [Required]
        public string Currency { get; set; } = string.Empty;
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public string AdjustmentType { get; set; } = string.Empty; // Credit, Debit, Correction
        
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        public string? Reference { get; set; }
        
        public string? AdminUserId { get; set; }
    }

    /// <summary>
    /// Response model for balance adjustments.
    /// </summary>
    public class BalanceAdjustmentResponse
    {
        public Guid AdjustmentId { get; set; }
        public Guid AccountId { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AdjustmentType { get; set; } = string.Empty;
        public decimal PreviousBalance { get; set; }
        public decimal NewBalance { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public string? AdminUserId { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
