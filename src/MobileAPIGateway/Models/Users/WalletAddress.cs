using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Users;

/// <summary>
/// Wallet address
/// </summary>
public sealed class WalletAddress
{
    /// <summary>
    /// Gets or sets the owner ID
    /// </summary>
    public string? OwnerId { get; set; }
    
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public MarketAsset? Currency { get; set; }
    
    /// <summary>
    /// Gets or sets the added date
    /// </summary>
    public DateTime? AddedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the crypto payout address
    /// </summary>
    public string? CryptoPayoutAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the logo
    /// </summary>
    public string? Logo { get; set; }
}
