namespace FeeService.Models.Responses;

public class FeeDistributionResponse
{
    public Guid Id { get; set; }
    public string RecipientType { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? TransactionHash { get; set; }
    public object? Metadata { get; set; }
    public Guid FeeTransactionId { get; set; }
    public FeeTransactionResponse? FeeTransaction { get; set; }
    public DistributionRuleResponse? DistributionRule { get; set; }
}
