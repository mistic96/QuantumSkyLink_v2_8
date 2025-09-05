using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Charge card request status
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChargeCardRequestStatus
{
    /// <summary>
    /// Pending status
    /// </summary>
    Pending,
    
    /// <summary>
    /// Canceled status
    /// </summary>
    Canceled,
    
    /// <summary>
    /// Disputed status
    /// </summary>
    Disputed,
    
    /// <summary>
    /// Completed status
    /// </summary>
    Completed,
    
    /// <summary>
    /// Not started status
    /// </summary>
    NotStarted
}
