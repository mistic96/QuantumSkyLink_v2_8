using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Preferred language
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PreferedLanguage
{
    /// <summary>
    /// English
    /// </summary>
    English,
    
    /// <summary>
    /// Spanish
    /// </summary>
    Español,
    
    /// <summary>
    /// Portuguese
    /// </summary>
    Português,
    
    /// <summary>
    /// Chinese
    /// </summary>
    中国的,
    
    /// <summary>
    /// Danish
    /// </summary>
    Dansk,
    
    /// <summary>
    /// Dutch
    /// </summary>
    Nederlands,
    
    /// <summary>
    /// French
    /// </summary>
    Français,
    
    /// <summary>
    /// German
    /// </summary>
    Deutsch,
    
    /// <summary>
    /// Hebrew
    /// </summary>
    עברית,
    
    /// <summary>
    /// Hindi
    /// </summary>
    हिंदी,
    
    /// <summary>
    /// Italian
    /// </summary>
    Italiano,
    
    /// <summary>
    /// Japanese
    /// </summary>
    日本の,
    
    /// <summary>
    /// Norwegian
    /// </summary>
    Norsk,
    
    /// <summary>
    /// Polish
    /// </summary>
    Polski,
    
    /// <summary>
    /// Romanian
    /// </summary>
    Român,
    
    /// <summary>
    /// Russian
    /// </summary>
    Русский,
    
    /// <summary>
    /// Slovak	
    /// </summary>
    Slovenský,
    
    /// <summary>
    /// Swedish
    /// </summary>
    Svenska
}
