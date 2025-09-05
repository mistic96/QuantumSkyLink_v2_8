namespace MobileAPIGateway.Models.Compatibility.Wallet;

/// <summary>
/// Request model for adding external wallet
/// </summary>
public class AddExternalWalletRequest
{
    /// <summary>
    /// External wallet address
    /// </summary>
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// Cryptocurrency type
    /// </summary>
    public string Cryptocurrency { get; set; } = string.Empty;

    /// <summary>
    /// Wallet name/label
    /// </summary>
    public string WalletName { get; set; } = string.Empty;

    /// <summary>
    /// Wallet type
    /// </summary>
    public string WalletType { get; set; } = string.Empty;
}
