using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Token type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TokenType
{
    /// <summary>
    /// Currency token
    /// </summary>
    Currency,
    
    /// <summary>
    /// FPT token
    /// </summary>
    FPT,
    
    /// <summary>
    /// SRWA token
    /// </summary>
    SRWA,
    
    /// <summary>
    /// Security token
    /// </summary>
    Security,
    
    /// <summary>
    /// ICO token
    /// </summary>
    ICO,
    
    /// <summary>
    /// Utility token
    /// </summary>
    Utility,
    
    /// <summary>
    /// Commodity token
    /// </summary>
    Commodity,
    
    /// <summary>
    /// Stable coin
    /// </summary>
    StableCoin
}
