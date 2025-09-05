using System.ComponentModel.DataAnnotations;

namespace InfrastructureService.Models.Requests;

/// <summary>
/// Request model for broadcasting a signed transaction to a blockchain network
/// </summary>
public class BroadcastSignedTransactionRequest
{
    /// <summary>
    /// The blockchain network to broadcast the transaction to (e.g., "Ethereum", "Tron", "Solana")
    /// </summary>
    [Required]
    public string Network { get; set; } = string.Empty;

    /// <summary>
    /// The raw signed transaction data to broadcast
    /// </summary>
    [Required]
    public string SignedTransaction { get; set; } = string.Empty;

    /// <summary>
    /// Optional metadata for tracking purposes
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
