namespace FeeService.Models.Responses;

public class SettlementResponse
{
    public Guid Id { get; set; }
    public IEnumerable<Guid> DistributionIds { get; set; } = new List<Guid>();
    public string SettlementMethod { get; set; } = string.Empty;
    public string? SettlementReference { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string ProcessedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? FailureReason { get; set; }
    public object? Metadata { get; set; }
    public IEnumerable<FeeDistributionResponse>? Distributions { get; set; }
}
