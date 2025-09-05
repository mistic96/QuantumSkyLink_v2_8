using System.ComponentModel.DataAnnotations;

namespace TreasuryService.Models.Requests;

public class UpdateTreasuryAccountRequest
{
    [MaxLength(100)]
    public string? AccountName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinimumBalance { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaximumBalance { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    [MaxLength(100)]
    public string? ExternalAccountId { get; set; }

    [MaxLength(100)]
    public string? BankName { get; set; }

    [MaxLength(50)]
    public string? RoutingNumber { get; set; }

    [MaxLength(50)]
    public string? SwiftCode { get; set; }

    public bool? IsDefault { get; set; }

    public bool? RequiresApproval { get; set; }

    [Range(0, 1)]
    public decimal? InterestRate { get; set; }

    public Guid? UpdatedBy { get; set; }
}

// Additional essential request models
public class GetBalanceHistoryRequest
{
    [Required]
    public Guid AccountId { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(20)]
    public string? BalanceType { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class ReconcileBalanceRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public decimal ActualBalance { get; set; }

    [Required]
    public decimal ExternalBalance { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    [MaxLength(500)]
    public string? ReconciliationNotes { get; set; }

    [MaxLength(100)]
    public string? ReconciliationReference { get; set; }
}

public class CreateTreasuryTransactionRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    public Guid? RelatedAccountId { get; set; }

    public Guid? CreatedBy { get; set; }
}

public class GetTreasuryTransactionsRequest
{
    public Guid? AccountId { get; set; }

    [MaxLength(50)]
    public string? TransactionType { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TransferFundsRequest
{
    [Required]
    public Guid FromAccountId { get; set; }

    [Required]
    public Guid ToAccountId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    public Guid? InitiatedBy { get; set; }
}

public class DepositFundsRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    public Guid? InitiatedBy { get; set; }

    public bool? AutoProcess { get; set; }
}

public class WithdrawFundsRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    public Guid? InitiatedBy { get; set; }

    public bool? AutoProcess { get; set; }
}

public class GetTreasuryAnalyticsRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(10)]
    public string? Currency { get; set; }

    [MaxLength(50)]
    public string? AccountType { get; set; }
}

public class GetCashFlowAnalysisRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(10)]
    public string? Currency { get; set; }

    public Guid? AccountId { get; set; }
}
