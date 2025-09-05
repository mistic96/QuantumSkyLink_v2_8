namespace FeeService.Models.Responses;

public class FeeTransactionResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal? ConvertedAmount { get; set; }
    public string? ConvertedCurrency { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public object? Metadata { get; set; }
    public FeeConfigurationResponse? FeeConfiguration { get; set; }
    public FeeCalculationResponse? CalculationResult { get; set; }
    public IEnumerable<FeeDistributionResponse>? Distributions { get; set; }
}
