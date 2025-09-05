using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.PrimaryMarkets;

/// <summary>
/// Token index milestone measurement
/// </summary>
public sealed class TokenIndexMileStoneMeasurement
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the unlocked measurement
    /// </summary>
    public decimal? UnlockedMeasurement { get; set; }
    
    /// <summary>
    /// Gets or sets the type
    /// </summary>
    public TokenIndexMileStoneMeasurementType? Type { get; set; }
    
    /// <summary>
    /// Gets or sets the measurement type parameter
    /// </summary>
    public TokenIndexMileStoneMeasurementTypeParameter? MeasurementTypeParameter { get; set; }
}
