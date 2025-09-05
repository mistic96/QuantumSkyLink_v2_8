using System.ComponentModel.DataAnnotations;

namespace TreasuryService.Models.Requests;

public class InternalTransferRequest
{
    [Required]
    public Guid FromAccountId { get; set; }
    
    [Required]
    public Guid ToAccountId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Reference { get; set; }
    
    public DateTime? ScheduledDate { get; set; }
    
    public bool RequiresApproval { get; set; } = false;
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public Guid? InitiatedBy { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ExternalTransferRequest
{
    [Required]
    public Guid FromAccountId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ToAccountNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string ToAccountName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string ToBankName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? ToBankCode { get; set; }
    
    [MaxLength(50)]
    public string? ToRoutingNumber { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Reference { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TransferType { get; set; } = string.Empty; // WIRE, ACH, SWIFT, etc.
    
    public DateTime? ScheduledDate { get; set; }
    
    public bool RequiresApproval { get; set; } = true;
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public Guid? ExternalAccountId { get; set; }
    
    public Guid? InitiatedBy { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ReconcileTransactionsRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ReconciliationType { get; set; } = string.Empty;
    
    public IEnumerable<ExternalTransactionRecord>? ExternalRecords { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public bool AutoResolveMatches { get; set; } = true;
    
    public decimal? ToleranceAmount { get; set; }
    
    public Dictionary<string, object>? ReconciliationRules { get; set; }
}

public class ExternalTransactionRecord
{
    [Required]
    [MaxLength(100)]
    public string ExternalId { get; set; } = string.Empty;
    
    [Required]
    public DateTime TransactionDate { get; set; }
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Reference { get; set; }
    
    [MaxLength(50)]
    public string? TransactionType { get; set; }
    
    [MaxLength(200)]
    public string? CounterpartyName { get; set; }
    
    [MaxLength(100)]
    public string? CounterpartyAccount { get; set; }
    
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class GetTransactionSummaryRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    
    public string[]? TransactionTypes { get; set; }
    
    public string[]? Statuses { get; set; }
    
    [MaxLength(50)]
    public string GroupBy { get; set; } = "Day"; // Day, Week, Month, Quarter, Year
    
    public bool IncludeSubAccounts { get; set; } = false;
    
    public decimal? MinAmount { get; set; }
    
    public decimal? MaxAmount { get; set; }
    
    public string? CounterpartyFilter { get; set; }
}

public class ValidateTransactionRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;
    
    public Guid? ToAccountId { get; set; }
    
    [MaxLength(200)]
    public string? ToAccountNumber { get; set; }
    
    public DateTime? ScheduledDate { get; set; }
    
    public bool CheckLimits { get; set; } = true;
    
    public bool CheckBalance { get; set; } = true;
    
    public bool CheckCompliance { get; set; } = true;
    
    public Dictionary<string, object>? AdditionalChecks { get; set; }
}

public class BulkTransactionRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    [MinLength(1)]
    public IEnumerable<BulkTransactionItem> Transactions { get; set; } = new List<BulkTransactionItem>();
    
    [MaxLength(100)]
    public string? BatchReference { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime? ScheduledDate { get; set; }
    
    public bool RequiresApproval { get; set; } = true;
    
    public bool ValidateAll { get; set; } = true;
    
    public bool StopOnFirstError { get; set; } = false;
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class BulkTransactionItem
{
    [Required]
    [MaxLength(100)]
    public string ItemReference { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;
    
    public Guid? ToAccountId { get; set; }
    
    [MaxLength(200)]
    public string? ToAccountNumber { get; set; }
    
    [MaxLength(200)]
    public string? ToAccountName { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Reference { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TransactionSearchRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    public DateTime? FromDate { get; set; }
    
    public DateTime? ToDate { get; set; }
    
    public string? Currency { get; set; }
    
    public string[]? TransactionTypes { get; set; }
    
    public string[]? Statuses { get; set; }
    
    public decimal? MinAmount { get; set; }
    
    public decimal? MaxAmount { get; set; }
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Reference { get; set; }
    
    [MaxLength(200)]
    public string? CounterpartyName { get; set; }
    
    [MaxLength(100)]
    public string? CounterpartyAccount { get; set; }
    
    public bool IncludeSubAccounts { get; set; } = false;
    
    [Range(1, 1000)]
    public int PageSize { get; set; } = 50;
    
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;
    
    [MaxLength(50)]
    public string SortBy { get; set; } = "TransactionDate";
    
    [MaxLength(10)]
    public string SortDirection { get; set; } = "DESC";
}

public class ApproveTransactionRequest
{
    [Required]
    public Guid TransactionId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // APPROVE, REJECT, REQUEST_CHANGES
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    [MaxLength(100)]
    public string? ApprovalCode { get; set; }
    
    public Guid? ApprovedBy { get; set; }
    
    public bool? AutoProcess { get; set; }
    
    [MaxLength(1000)]
    public string? ApprovalNotes { get; set; }
    
    public Dictionary<string, object>? ApprovalMetadata { get; set; }
}

public class CancelTransactionRequest
{
    [Required]
    public Guid TransactionId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string CancellationReason { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public bool ForceCancel { get; set; } = false;
}

public class RetryTransactionRequest
{
    [Required]
    public Guid TransactionId { get; set; }
    
    [MaxLength(500)]
    public string? RetryReason { get; set; }
    
    public bool UpdateAmount { get; set; } = false;
    
    public decimal? NewAmount { get; set; }
    
    public bool UpdateScheduledDate { get; set; } = false;
    
    public DateTime? NewScheduledDate { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class TransactionLimitCheckRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string LimitType { get; set; } = "DAILY"; // DAILY, WEEKLY, MONTHLY, ANNUAL, TRANSACTION
    
    public DateTime? TransactionDate { get; set; }
    
    public bool IncludePendingTransactions { get; set; } = true;
}

public class ScheduleTransactionRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;
    
    [Required]
    public DateTime ScheduledDate { get; set; }
    
    public Guid? ToAccountId { get; set; }
    
    [MaxLength(200)]
    public string? ToAccountNumber { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Reference { get; set; }
    
    [MaxLength(50)]
    public string RecurrencePattern { get; set; } = "NONE"; // NONE, DAILY, WEEKLY, MONTHLY, QUARTERLY, ANNUALLY
    
    public DateTime? RecurrenceEndDate { get; set; }
    
    public int? RecurrenceCount { get; set; }
    
    public bool RequiresApproval { get; set; } = false;
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class TransactionReportRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ReportType { get; set; } = string.Empty;
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    
    public string[]? TransactionTypes { get; set; }
    
    public string[]? Statuses { get; set; }
    
    [MaxLength(50)]
    public string Format { get; set; } = "PDF"; // PDF, EXCEL, CSV, JSON
    
    public bool IncludeCharts { get; set; } = true;
    
    public bool IncludeSummary { get; set; } = true;
    
    public bool IncludeDetails { get; set; } = true;
    
    public string[]? Recipients { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
