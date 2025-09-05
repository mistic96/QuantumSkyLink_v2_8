using System.Text.Json.Serialization;
using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Cart;

/// <summary>
/// Cloud exchange rate
/// </summary>
public sealed class CloudExchangeRate
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the currency type
    /// </summary>
    [JsonPropertyName("currencyName")]
    public CloudCartCurrencyType CurrencyType { get; set; }

    /// <summary>
    /// Gets or sets the index date
    /// </summary>
    public DateTimeOffset IndexDate { get; set; }

    /// <summary>
    /// Gets or sets the currency rate
    /// </summary>
    public decimal? CurrencyRate { get; set; }
    
    /// <summary>
    /// Gets or sets the price in USD
    /// </summary>
    [JsonPropertyName("usdValue")]
    public decimal PriceInUsd { get; set; }

    /// <summary>
    /// Gets or sets the quote ID
    /// </summary>
    public string? QuoteId { get; set; }
}
