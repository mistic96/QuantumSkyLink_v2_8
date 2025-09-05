namespace FeeService.Models.Responses;

public class ExchangeRateResponse
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Provider { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public decimal? Bid { get; set; }
    public decimal? Ask { get; set; }
    public decimal? Volume24h { get; set; }
    public decimal? Change24h { get; set; }
}
