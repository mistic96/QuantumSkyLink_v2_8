namespace FeeService.Models.Responses;

public class FeeConfigurationResponse
{
    public Guid Id { get; set; }
    public string FeeType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string CalculationType { get; set; } = string.Empty;
    public decimal? FlatFeeAmount { get; set; }
    public decimal? PercentageRate { get; set; }
    public decimal? MinimumFee { get; set; }
    public decimal? MaximumFee { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveUntil { get; set; }
    public string? Description { get; set; }
    public object? TieredStructure { get; set; }
    public object? DiscountRules { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}
