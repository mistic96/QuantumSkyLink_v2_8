using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Pricing model
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PricingModel
{
    /// <summary>
    /// None
    /// </summary>
    None,
    
    /// <summary>
    /// Bulk pricing
    /// </summary>
    Bulk,
    
    /// <summary>
    /// Unit pricing
    /// </summary>
    Unit,
    
    /// <summary>
    /// Margin pricing
    /// </summary>
    Margin,
    
    /// <summary>
    /// Dynamic pricing
    /// </summary>
    Dynamic
}
