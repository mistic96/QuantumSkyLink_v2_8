namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// Public team extended price
/// </summary>
public sealed class PublicTeamExtendedPrice
{
    /// <summary>
    /// Gets or sets the team
    /// </summary>
    public PublicTeam? Team { get; set; }
    
    /// <summary>
    /// Gets or sets the price
    /// </summary>
    public TeamPrice? Price { get; set; }
}
