namespace TreasuryService.Models.Responses;

public class TreasuryBalanceResponse
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string BalanceType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal ReservedBalance { get; set; }
    public decimal PendingCredits { get; set; }
    public decimal PendingDebits { get; set; }
    public decimal DayChange { get; set; }
    public decimal? DayChangePercentage { get; set; }
    public DateTime BalanceDate { get; set; }
    public DateTime AsOfTime { get; set; }
    public DateTime LastUpdated { get; set; }
    public decimal MinimumBalance { get; set; }
    public decimal MaximumBalance { get; set; }
    public decimal ChangeAmount { get; set; }
    public string? ChangeReason { get; set; }
    public Guid? TransactionId { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public string? ReconciliationNotes { get; set; }
}

public class TreasuryTransactionResponse
{
    public Guid Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? SettledDate { get; set; }
    public string? CounterpartyName { get; set; }
    public string? CounterpartyAccount { get; set; }
    public Guid? RelatedAccountId { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal? FeeAmount { get; set; }
    public bool RequiresApproval { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}

public class TreasuryAnalyticsResponse
{
    public decimal TotalBalance { get; set; }
    public decimal TotalAvailableBalance { get; set; }
    public decimal TotalReservedBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime AsOfDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int AccountCount { get; set; }
    public int TotalAccounts { get; set; }
    public int ActiveAccounts { get; set; }
    public int TransactionCount { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalInflows { get; set; }
    public decimal TotalOutflows { get; set; }
    public decimal NetCashFlow { get; set; }
    public decimal TotalTransactionVolume { get; set; }
    public decimal AverageBalance { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public decimal HighestBalance { get; set; }
    public decimal LowestBalance { get; set; }
    public decimal LargestTransaction { get; set; }
    public Dictionary<string, int> TransactionsByType { get; set; } = new();
    public Dictionary<string, int> TransactionsByStatus { get; set; } = new();
    public IEnumerable<AccountTypeBalance> BalancesByAccountType { get; set; } = new List<AccountTypeBalance>();
    public IEnumerable<DailyBalance> DailyBalances { get; set; } = new List<DailyBalance>();
}

public class AccountTypeBalance
{
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal AvailableBalance { get; set; }
    public int AccountCount { get; set; }
}

public class DailyBalance
{
    public DateTime Date { get; set; }
    public decimal Balance { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercentage { get; set; }
}

public class TreasuryAccountSummaryResponse
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal ReservedBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime LastTransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TransactionCount { get; set; }
    public decimal DayChange { get; set; }
    public decimal? DayChangePercentage { get; set; }
}

public class TreasuryCashFlowResponse
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal TotalInflows { get; set; }
    public decimal TotalOutflows { get; set; }
    public decimal NetCashFlow { get; set; }
    public decimal AverageDailyBalance { get; set; }
    public IEnumerable<CashFlowByType> InflowsByType { get; set; } = new List<CashFlowByType>();
    public IEnumerable<CashFlowByType> OutflowsByType { get; set; } = new List<CashFlowByType>();
    public IEnumerable<DailyCashFlow> DailyCashFlows { get; set; } = new List<DailyCashFlow>();
}

public class CashFlowByType
{
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
}

public class DailyCashFlow
{
    public DateTime Date { get; set; }
    public decimal Inflows { get; set; }
    public decimal Outflows { get; set; }
    public decimal NetFlow { get; set; }
    public decimal Balance { get; set; }
}
