namespace MobileAPIGateway.Models.Compatibility.Wallet;

/// <summary>
/// Response model for adding external wallet
/// </summary>
public class AddExternalWalletResponse
{
    /// <summary>
    /// Wallet identifier
    /// </summary>
    public string WalletId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Operation message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
