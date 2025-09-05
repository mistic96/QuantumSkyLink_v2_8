using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Order request type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderRequestType
{
    /// <summary>
    /// None
    /// </summary>
    None,
    
    /// <summary>
    /// Retail
    /// </summary>
    Retail,
    
    /// <summary>
    /// Token
    /// </summary>
    Token,
    
    /// <summary>
    /// NFT
    /// </summary>
    Nft,
    
    /// <summary>
    /// Subscription
    /// </summary>
    Subscription,
    
    /// <summary>
    /// Transfer
    /// </summary>
    Transfer,
    
    /// <summary>
    /// Services collection
    /// </summary>
    ServicesCollection
}
