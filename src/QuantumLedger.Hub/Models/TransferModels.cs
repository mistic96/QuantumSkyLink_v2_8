using System.ComponentModel.DataAnnotations;
using QuantumLedger.Hub.Models;

namespace QuantumLedger.Hub.Models
{
    /// <summary>
    /// Request model for fund transfers between accounts.
    /// </summary>
    public class TransferRequest
    {
        [Required]
        public Guid FromAccountId { get; set; }
        
        [Required]
        public Guid ToAccountId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        public string? Currency { get; set; }
        
        public string? Description { get; set; }
        
        public string? Reference { get; set; }
    }

    /// <summary>
    /// Response model for completed transfers.
    /// </summary>
    public class TransferResponse
    {
        public Guid TransactionId { get; set; }
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? BlockHash { get; set; }
        public decimal TransactionFee { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string? ConfirmationNumber { get; set; }
    }

    /// <summary>
    /// Request model for batch transfers.
    /// </summary>
    public class BatchTransferRequest
    {
        [Required]
        public List<TransferRequest> Transfers { get; set; } = new();
        
        public string? Description { get; set; }
        
        public bool AtomicExecution { get; set; } = true;
    }

    /// <summary>
    /// Response model for batch transfer operations.
    /// </summary>
    public class BatchTransferResponse
    {
        public Guid BatchId { get; set; }
        public int TotalTransfers { get; set; }
        public int SuccessfulTransfers { get; set; }
        public int FailedTransfers { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalFees { get; set; }
        public DateTime ProcessedAt { get; set; }
        public List<BatchTransferResult> Results { get; set; } = new();
    }

    /// <summary>
    /// Individual result within a batch transfer.
    /// </summary>
    public class BatchTransferResult
    {
        public Guid TransactionId { get; set; }
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? BlockHash { get; set; }
        public decimal TransactionFee { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
