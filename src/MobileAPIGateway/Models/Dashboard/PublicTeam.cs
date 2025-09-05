namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// Public team
/// </summary>
public sealed class PublicTeam
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the club rank
    /// </summary>
    public int ClubRank { get; set; }
    
    /// <summary>
    /// Gets or sets the country ID
    /// </summary>
    public int CountryId { get; set; }
    
    /// <summary>
    /// Gets or sets the blockchain alias
    /// </summary>
    public string BlockChainAlisa { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the logo
    /// </summary>
    public string Logo { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the points
    /// </summary>
    public int Points { get; set; }
    
    /// <summary>
    /// Gets or sets the rank
    /// </summary>
    public int Rank { get; set; }
    
    /// <summary>
    /// Gets or sets the available
    /// </summary>
    public decimal Available { get; set; }
    
    /// <summary>
    /// Gets or sets the assigned
    /// </summary>
    public decimal Assigned { get; set; }
    
    /// <summary>
    /// Gets or sets the reserved
    /// </summary>
    public decimal? Reserved { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether sale is available
    /// </summary>
    public bool IsSaleAvailable { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether is contracted
    /// </summary>
    public bool IsContracted { get; set; }
    
    /// <summary>
    /// Gets or sets the market symbol
    /// </summary>
    public string? MarketSymbol { get; set; }
    
    /// <summary>
    /// Gets or sets the liquidity open date
    /// </summary>
    public DateTime? LiquidityOpenDate { get; set; }
}
