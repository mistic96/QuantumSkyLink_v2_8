namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// Wallet extended
/// </summary>
public sealed class WalletExtended
{
    /// <summary>
    /// Gets or sets the in wallet
    /// </summary>
    public WalletAssets? InWallet { get; set; }
    
    /// <summary>
    /// Gets or sets the on hold
    /// </summary>
    public WalletAssets? OnHold { get; set; }
}
