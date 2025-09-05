using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Network interfaces
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NetworkInterfaces
{
    /// <summary>
    /// Football exchange
    /// </summary>
    FootballExchange,
    
    /// <summary>
    /// Global coin exchange
    /// </summary>
    GlobalCoinExchange,
    
    /// <summary>
    /// XLiquidus exchange
    /// </summary>
    XLiquidusExchange,
    
    /// <summary>
    /// Player exchange
    /// </summary>
    PlayerExchange,
    
    /// <summary>
    /// Cricket exchange
    /// </summary>
    CricketExchange,
    
    /// <summary>
    /// USS Cyber
    /// </summary>
    UssCyber
}
