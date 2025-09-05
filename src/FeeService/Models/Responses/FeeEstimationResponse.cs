namespace FeeService.Models.Responses;

public class FeeEstimationResponse
{
    public string FeeType { get; set; } = string.Empty;
    public decimal BaseAmount { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public decimal EstimatedFee { get; set; }
    public string FeeCurrency { get; set; } = string.Empty;
    public decimal? PotentialDiscount { get; set; }
    public decimal? PotentialDiscountPercentage { get; set; }
    public string? DiscountReason { get; set; }
    public decimal FinalEstimatedFee { get; set; }
    public decimal? ExchangeRate { get; set; }
    public DateTime EstimatedAt { get; set; }
    public object? EstimationDetails { get; set; }
    public FeeConfigurationResponse? AppliedConfiguration { get; set; }
    public bool IsExactCalculation { get; set; } // True if user-specific, false if generic estimation
}
