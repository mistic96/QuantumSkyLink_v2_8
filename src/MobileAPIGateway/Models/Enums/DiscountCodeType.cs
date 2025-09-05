using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Discount code type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DiscountCodeType
{
    /// <summary>
    /// Campaign management
    /// </summary>
    CampaignManagement,
    
    /// <summary>
    /// Client referral program
    /// </summary>
    ClientReferralProgram
}
