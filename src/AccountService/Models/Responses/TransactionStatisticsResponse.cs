using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class TransactionStatisticsResponse
{
    public Guid AccountId { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public int TotalTransactions { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalVolume { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal AverageTransactionAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal LargestTransaction { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal SmallestTransaction { get; set; }

    public int DepositCount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal DepositVolume { get; set; }

    public int WithdrawalCount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal WithdrawalVolume { get; set; }

    public int TransferCount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal TransferVolume { get; set; }

    public int FailedTransactions { get; set; }

    public int PendingTransactions { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalFees { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalDeposits { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalWithdrawals { get; set; }
}
