namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// Response model for user synchronization
/// </summary>
public class UserSyncResponse
{
    /// <summary>
    /// Communication access key
    /// </summary>
    public string CommunicationAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Synchronization status
    /// </summary>
    public string SyncStatus { get; set; } = string.Empty;
}
