namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// Request model for user synchronization
/// </summary>
public class UserSyncRequest
{
    /// <summary>
    /// User object identifier
    /// </summary>
    public string UserObjectId { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Request date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Application identifier
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// Device identifier
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Device operating system
    /// </summary>
    public string DeviceOs { get; set; } = string.Empty;

    /// <summary>
    /// Device name
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// Location geo tag
    /// </summary>
    public string LocationGeoTag { get; set; } = string.Empty;

    /// <summary>
    /// Request signature
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Request status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Sync type (ReSync, Sync, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Network interface
    /// </summary>
    public string NetworkInterface { get; set; } = string.Empty;

    /// <summary>
    /// Request nonce
    /// </summary>
    public long Nonce { get; set; }
}
