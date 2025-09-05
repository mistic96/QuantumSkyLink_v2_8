using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Cloud cart checked out status
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CloudCartCheckedOutStatus
{
    /// <summary>
    /// Pending status
    /// </summary>
    Pending,
    
    /// <summary>
    /// Waiting on payment status
    /// </summary>
    WaitingOnPayment,
    
    /// <summary>
    /// Paid status
    /// </summary>
    Paid,
    
    /// <summary>
    /// Processing status
    /// </summary>
    Processing,
    
    /// <summary>
    /// Completed status
    /// </summary>
    Completed,
    
    /// <summary>
    /// Canceled status
    /// </summary>
    Canceled,
    
    /// <summary>
    /// Create charge failed status
    /// </summary>
    CreateChargeFailed
}
