namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// Balance metric
/// </summary>
public sealed class BalanceMetric
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Gets or sets the review date
    /// </summary>
    public DateTime ReviewDate { get; set; }
    
    /// <summary>
    /// Gets or sets the total wallet balance
    /// </summary>
    public decimal TotalWalletBalance { get; set; }
}
