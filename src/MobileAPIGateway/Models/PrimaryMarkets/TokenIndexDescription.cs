namespace MobileAPIGateway.Models.PrimaryMarkets;

/// <summary>
/// Token index description
/// </summary>
public sealed class TokenIndexDescription
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the summary
    /// </summary>
    public string? Summary { get; set; }
    
    /// <summary>
    /// Gets or sets the entry date UTC
    /// </summary>
    public DateTime EntryDateUtc { get; set; } = DateTime.UtcNow;
}
