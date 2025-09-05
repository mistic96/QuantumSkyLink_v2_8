namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// Team price
/// </summary>
public sealed class TeamPrice
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the current price
    /// </summary>
    public decimal CurrentPrice { get; set; } = 0.00m;
    
    /// <summary>
    /// Gets or sets the previous price
    /// </summary>
    public decimal PreviousPrice { get; set; } = 0.00m;
    
    /// <summary>
    /// Gets or sets the price flux percentage
    /// </summary>
    public PercentageFlux? PriceFluxPercentage { get; set; }
    
    /// <summary>
    /// Gets or sets the quote ID
    /// </summary>
    public string? QuoteId { get; set; }
    
    /// <summary>
    /// Gets or sets the country ID
    /// </summary>
    public int CountryId { get; set; }
    
    /// <summary>
    /// Gets or sets the team ID
    /// </summary>
    public int TeamId { get; set; }
    
    /// <summary>
    /// Gets or sets the blockchain alias
    /// </summary>
    public string? BlockchainAlias { get; set; }
    
    /// <summary>
    /// Gets or sets the update on
    /// </summary>
    public DateTime? UpdateOn { get; set; }
    
    /// <summary>
    /// Gets or sets the FX rate
    /// </summary>
    public FxRate? FxRate { get; set; }
}
