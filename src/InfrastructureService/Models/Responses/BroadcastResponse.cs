namespace InfrastructureService.Models.Responses;

/// <summary>
/// Response model for broadcasting a signed transaction
/// </summary>
public class BroadcastResponse
{
    /// <summary>
    /// The transaction hash returned by the blockchain network
    /// </summary>
    public string TxHash { get; set; } = string.Empty;

    /// <summary>
    /// The status of the broadcast operation (e.g., "broadcasted", "failed")
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Optional error message if the broadcast failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The blockchain network the transaction was broadcast to
    /// </summary>
    public string? Network { get; set; }

    /// <summary>
    /// Timestamp when the broadcast occurred
    /// </summary>
    public DateTime BroadcastTime { get; set; } = DateTime.UtcNow;
}
