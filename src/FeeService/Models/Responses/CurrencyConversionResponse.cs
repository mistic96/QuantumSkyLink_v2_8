namespace FeeService.Models.Responses;

public class CurrencyConversionResponse
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public string Provider { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public decimal? Fee { get; set; }
    public string? FeeCurrency { get; set; }
}
