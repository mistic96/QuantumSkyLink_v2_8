namespace FeeService.Models.Responses;

public class DistributionRuleResponse
{
    public Guid Id { get; set; }
    public string FeeType { get; set; } = string.Empty;
    public string RecipientType { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveUntil { get; set; }
    public string? Description { get; set; }
    public object? Conditions { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public string? Currency { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public IEnumerable<FeeDistributionResponse>? FeeDistributions { get; set; }
}
