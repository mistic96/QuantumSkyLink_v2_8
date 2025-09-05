namespace FeeService.Models.Responses;

public class FeeCalculationResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FeeType { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public decimal BaseAmount { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public decimal CalculatedFee { get; set; }
    public string FeeCurrency { get; set; } = string.Empty;
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public string? DiscountReason { get; set; }
    public decimal FinalFeeAmount { get; set; }
    public decimal? UsedExchangeRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public object? CalculationDetails { get; set; }
    public object? AppliedRules { get; set; }
    public FeeConfigurationResponse? FeeConfiguration { get; set; }
    public ExchangeRateResponse? ExchangeRate { get; set; }
}
