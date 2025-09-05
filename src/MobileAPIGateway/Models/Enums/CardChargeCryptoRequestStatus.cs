using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Card charge crypto request status
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CardChargeCryptoRequestStatus
{
    /// <summary>
    /// Pending
    /// </summary>
    Pending,
    
    /// <summary>
    /// Canceled
    /// </summary>
    Canceled,
    
    /// <summary>
    /// Disputed
    /// </summary>
    Disputed,
    
    /// <summary>
    /// Completed
    /// </summary>
    Completed,
    
    /// <summary>
    /// Waiting for payment
    /// </summary>
    WaitingForPayment,
    
    /// <summary>
    /// Failed
    /// </summary>
    Failed
}
