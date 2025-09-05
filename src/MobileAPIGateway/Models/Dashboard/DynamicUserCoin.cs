namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// Dynamic user coin
/// </summary>
public sealed class DynamicUserCoin
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Gets or sets the team ID
    /// </summary>
    public long TeamId { get; set; }
    
    /// <summary>
    /// Gets or sets the team name
    /// </summary>
    public string TeamName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the logo
    /// </summary>
    public string Logo { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the average cost
    /// </summary>
    public decimal AverageCost { get; set; }
    
    /// <summary>
    /// Gets or sets the price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the current token price
    /// </summary>
    public decimal CurrentTokenPrice { get; set; }
}
