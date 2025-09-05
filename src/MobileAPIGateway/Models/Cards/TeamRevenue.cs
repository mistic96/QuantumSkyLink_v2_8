using System.ComponentModel.DataAnnotations.Schema;

namespace MobileAPIGateway.Models.Cards;

/// <summary>
/// Team revenue
/// </summary>
public sealed class TeamRevenue
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
    /// Gets or sets the amount
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }
}
