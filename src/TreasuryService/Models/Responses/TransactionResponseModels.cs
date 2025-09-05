namespace TreasuryService.Models.Responses;

public class TransactionValidationResponse
{
    public bool IsValid { get; set; }
    public IEnumerable<ValidationResult> ValidationResults { get; set; } = new List<ValidationResult>();
    public IEnumerable<LimitCheckResult> LimitChecks { get; set; } = new List<LimitCheckResult>();
    public BalanceCheckResult? BalanceCheck { get; set; }
    public ComplianceCheckResult? ComplianceCheck { get; set; }
    public decimal EstimatedFees { get; set; }
    public DateTime? EstimatedProcessingTime { get; set; }
    public IEnumerable<ValidationWarning> Warnings { get; set; } = new List<ValidationWarning>();
    public string OverallStatus { get; set; } = string.Empty;
    public string? RecommendedAction { get; set; }
}

public class ValidationResult
{
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? ErrorMessage { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

public class LimitCheckResult
{
    public string LimitType { get; set; } = string.Empty;
    public decimal LimitAmount { get; set; }
    public decimal UsedAmount { get; set; }
    public decimal AvailableAmount { get; set; }
    public decimal RequestedAmount { get; set; }
    public bool WithinLimit { get; set; }
    public decimal ExcessAmount { get; set; }
    public string Period { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class BalanceCheckResult
{
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal EstimatedFees { get; set; }
    public decimal TotalRequired { get; set; }
    public bool SufficientFunds { get; set; }
    public decimal Shortfall { get; set; }
    public IEnumerable<string> ReservedAmounts { get; set; } = new List<string>();
}

public class ComplianceCheckResult
{
    public bool IsCompliant { get; set; }
    public IEnumerable<ComplianceRule> RulesChecked { get; set; } = new List<ComplianceRule>();
    public IEnumerable<ComplianceViolation> Violations { get; set; } = new List<ComplianceViolation>();
    public string RiskLevel { get; set; } = string.Empty;
    public bool RequiresAdditionalReview { get; set; }
    public string? ReviewReason { get; set; }
}

public class ValidationWarning
{
    public string WarningType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
    public bool CanProceed { get; set; }
}

public class ReconciliationResponse
{
    public Guid ReconciliationId { get; set; }
    public Guid AccountId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ReconciliationType { get; set; } = string.Empty;
    public DateTime ReconciliationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public ReconciliationSummary Summary { get; set; } = new();
    public IEnumerable<ReconciliationMatch> Matches { get; set; } = new List<ReconciliationMatch>();
    public IEnumerable<ReconciliationException> Exceptions { get; set; } = new List<ReconciliationException>();
    public IEnumerable<ReconciliationAdjustment> Adjustments { get; set; } = new List<ReconciliationAdjustment>();
    public TimeSpan ProcessingTime { get; set; }
    public string? Notes { get; set; }
}

public class ReconciliationSummary
{
    public int TotalInternalTransactions { get; set; }
    public int TotalExternalRecords { get; set; }
    public int MatchedTransactions { get; set; }
    public int UnmatchedInternal { get; set; }
    public int UnmatchedExternal { get; set; }
    public int ExceptionsFound { get; set; }
    public int AdjustmentsMade { get; set; }
    public decimal TotalInternalAmount { get; set; }
    public decimal TotalExternalAmount { get; set; }
    public decimal VarianceAmount { get; set; }
    public decimal MatchRate { get; set; }
    public bool IsReconciled { get; set; }
}

public class ReconciliationMatch
{
    public Guid InternalTransactionId { get; set; }
    public string ExternalRecordId { get; set; } = string.Empty;
    public string MatchType { get; set; } = string.Empty;
    public decimal MatchConfidence { get; set; }
    public DateTime MatchDate { get; set; }
    public decimal InternalAmount { get; set; }
    public decimal ExternalAmount { get; set; }
    public decimal Variance { get; set; }
    public bool IsAutoMatched { get; set; }
    public string? MatchReason { get; set; }
}

public class ReconciliationException
{
    public Guid ExceptionId { get; set; }
    public string ExceptionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public Guid? TransactionId { get; set; }
    public string? ExternalRecordId { get; set; }
    public decimal? Amount { get; set; }
    public DateTime DetectedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class ReconciliationAdjustment
{
    public Guid AdjustmentId { get; set; }
    public string AdjustmentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid? TransactionId { get; set; }
    public string? Reference { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public Guid CreatedBy { get; set; }
    public string? Reason { get; set; }
}

public class TransactionStatisticsResponse
{
    public Guid AccountId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public TransactionCounts Counts { get; set; } = new();
    public TransactionAmounts Amounts { get; set; } = new();
    public IEnumerable<TransactionTypeStatistic> ByType { get; set; } = new List<TransactionTypeStatistic>();
    public IEnumerable<TransactionStatusStatistic> ByStatus { get; set; } = new List<TransactionStatusStatistic>();
    public IEnumerable<DailyTransactionStatistic> DailyBreakdown { get; set; } = new List<DailyTransactionStatistic>();
    public TransactionTrends Trends { get; set; } = new();
}

public class TransactionCounts
{
    public int TotalTransactions { get; set; }
    public int SuccessfulTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public int PendingTransactions { get; set; }
    public int CancelledTransactions { get; set; }
    public int InternalTransfers { get; set; }
    public int ExternalTransfers { get; set; }
    public int ScheduledTransactions { get; set; }
    public int RecurringTransactions { get; set; }
}

public class TransactionAmounts
{
    public decimal TotalAmount { get; set; }
    public decimal TotalInflows { get; set; }
    public decimal TotalOutflows { get; set; }
    public decimal NetAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal MedianAmount { get; set; }
    public decimal LargestAmount { get; set; }
    public decimal SmallestAmount { get; set; }
    public decimal TotalFees { get; set; }
    public decimal StandardDeviation { get; set; }
}

public class TransactionTypeStatistic
{
    public string TransactionType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal Percentage { get; set; }
    public decimal SuccessRate { get; set; }
}

public class TransactionStatusStatistic
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
    public decimal AverageProcessingTime { get; set; }
}

public class DailyTransactionStatistic
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Inflows { get; set; }
    public decimal Outflows { get; set; }
    public decimal NetAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
}

public class TransactionTrends
{
    public string Volumetrend { get; set; } = string.Empty;
    public string AmountTrend { get; set; } = string.Empty;
    public decimal VolumeGrowthRate { get; set; }
    public decimal AmountGrowthRate { get; set; }
    public decimal SuccessRateTrend { get; set; }
    public string PeakDay { get; set; } = string.Empty;
    public string PeakHour { get; set; } = string.Empty;
    public IEnumerable<TrendIndicator> Indicators { get; set; } = new List<TrendIndicator>();
}

public class TransactionSummaryResponse
{
    public Guid AccountId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string GroupBy { get; set; } = string.Empty;
    public IEnumerable<TransactionSummaryItem> Summary { get; set; } = new List<TransactionSummaryItem>();
    public TransactionSummaryTotals Totals { get; set; } = new();
    public IEnumerable<TransactionSummaryChart> Charts { get; set; } = new List<TransactionSummaryChart>();
}

public class TransactionSummaryItem
{
    public string Period { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Inflows { get; set; }
    public decimal Outflows { get; set; }
    public decimal NetAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal LargestTransaction { get; set; }
    public decimal SmallestTransaction { get; set; }
    public int SuccessfulTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public decimal SuccessRate { get; set; }
}

public class TransactionSummaryTotals
{
    public int TotalTransactions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalInflows { get; set; }
    public decimal TotalOutflows { get; set; }
    public decimal NetAmount { get; set; }
    public decimal OverallSuccessRate { get; set; }
    public decimal AverageTransactionSize { get; set; }
    public int TotalPeriods { get; set; }
}

public class TransactionSummaryChart
{
    public string ChartType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IEnumerable<ChartDataPoint> DataPoints { get; set; } = new List<ChartDataPoint>();
    public Dictionary<string, object> Options { get; set; } = new();
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime? Date { get; set; }
    public string? Category { get; set; }
    public string? Color { get; set; }
}

public class BulkTransactionResponse
{
    public Guid BatchId { get; set; }
    public Guid AccountId { get; set; }
    public string BatchReference { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public BulkTransactionSummary Summary { get; set; } = new();
    public IEnumerable<BulkTransactionResult> Results { get; set; } = new List<BulkTransactionResult>();
    public IEnumerable<BulkTransactionError> Errors { get; set; } = new List<BulkTransactionError>();
    public TimeSpan ProcessingTime { get; set; }
    public string? Notes { get; set; }
}

public class BulkTransactionSummary
{
    public int TotalItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public int SkippedItems { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public decimal FailedAmount { get; set; }
    public decimal SuccessRate { get; set; }
    public bool OverallSuccess { get; set; }
}

public class BulkTransactionResult
{
    public string ItemReference { get; set; } = string.Empty;
    public Guid? TransactionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TransactionNumber { get; set; }
}

public class BulkTransactionError
{
    public string ItemReference { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public string? FieldValue { get; set; }
    public string Severity { get; set; } = string.Empty;
    public bool CanRetry { get; set; }
}

public class TransactionSearchResponse
{
    public IEnumerable<TransactionSearchResult> Transactions { get; set; } = new List<TransactionSearchResult>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public TransactionSearchSummary Summary { get; set; } = new();
    public IEnumerable<SearchFacet> Facets { get; set; } = new List<SearchFacet>();
}

public class TransactionSearchResult
{
    public Guid TransactionId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public string? CounterpartyName { get; set; }
    public string? CounterpartyAccount { get; set; }
    public decimal? FeeAmount { get; set; }
    public bool RequiresApproval { get; set; }
    public string? ApprovalStatus { get; set; }
}

public class TransactionSearchSummary
{
    public decimal TotalAmount { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal LargestAmount { get; set; }
    public decimal SmallestAmount { get; set; }
    public IEnumerable<string> TransactionTypes { get; set; } = new List<string>();
    public IEnumerable<string> Statuses { get; set; } = new List<string>();
    public IEnumerable<string> Currencies { get; set; } = new List<string>();
}

public class SearchFacet
{
    public string FacetName { get; set; } = string.Empty;
    public IEnumerable<FacetValue> Values { get; set; } = new List<FacetValue>();
}

public class FacetValue
{
    public string Value { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool IsSelected { get; set; }
}

public class TransactionApprovalResponse
{
    public Guid TransactionId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
    public Guid ApprovedBy { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? ApprovalCode { get; set; }
    public bool RequiresAdditionalApproval { get; set; }
    public IEnumerable<string> NextApprovers { get; set; } = new List<string>();
    public ApprovalWorkflow? Workflow { get; set; }
}

public class ApprovalWorkflow
{
    public Guid WorkflowId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public IEnumerable<ApprovalStep> Steps { get; set; } = new List<ApprovalStep>();
    public string Status { get; set; } = string.Empty;
}

public class ApprovalStep
{
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string ApproverRole { get; set; } = string.Empty;
    public Guid? ApproverId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsRequired { get; set; }
}

public class TransactionLimitCheckResponse
{
    public Guid AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public IEnumerable<LimitCheckResult> LimitChecks { get; set; } = new List<LimitCheckResult>();
    public bool OverallResult { get; set; }
    public decimal TotalAvailableLimit { get; set; }
    public decimal TotalUsedLimit { get; set; }
    public IEnumerable<LimitRecommendation> Recommendations { get; set; } = new List<LimitRecommendation>();
    public DateTime CheckDate { get; set; }
}

public class LimitRecommendation
{
    public string RecommendationType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public decimal? SuggestedAmount { get; set; }
    public DateTime? SuggestedDate { get; set; }
    public string? Action { get; set; }
}

public class ScheduledTransactionResponse
{
    public Guid ScheduleId { get; set; }
    public Guid AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string RecurrencePattern { get; set; } = string.Empty;
    public DateTime? RecurrenceEndDate { get; set; }
    public int? RecurrenceCount { get; set; }
    public int ExecutedCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastExecuted { get; set; }
    public DateTime? NextExecution { get; set; }
    public IEnumerable<ScheduledTransactionExecution> Executions { get; set; } = new List<ScheduledTransactionExecution>();
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public bool RequiresApproval { get; set; }
}

public class ScheduledTransactionExecution
{
    public Guid ExecutionId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? ExecutedDate { get; set; }
    public Guid? TransactionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionNumber { get; set; }
}

public class TransactionReportResponse
{
    public Guid ReportId { get; set; }
    public Guid AccountId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string Format { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Status { get; set; } = string.Empty;
    public TransactionReportSummary Summary { get; set; } = new();
    public IEnumerable<ReportSection> Sections { get; set; } = new List<ReportSection>();
    public TimeSpan GenerationTime { get; set; }
}

public class TransactionReportSummary
{
    public int TotalTransactions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalInflows { get; set; }
    public decimal TotalOutflows { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TotalFees { get; set; }
    public int SuccessfulTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public decimal SuccessRate { get; set; }
    public string Currency { get; set; } = string.Empty;
}
