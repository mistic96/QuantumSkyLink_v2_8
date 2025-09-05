using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Fee type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FeeType
{
    /// <summary>
    /// Card fee
    /// </summary>
    Card,
    
    /// <summary>
    /// KYC fee
    /// </summary>
    KYC,
    
    /// <summary>
    /// Network fee
    /// </summary>
    Network,
    
    /// <summary>
    /// Vendor fee
    /// </summary>
    Vendor
}
