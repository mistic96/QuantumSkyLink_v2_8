using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// Percentage flux
/// </summary>
public sealed class PercentageFlux
{
    /// <summary>
    /// Gets or sets the result
    /// </summary>
    public decimal Result { get; set; }
    
    /// <summary>
    /// Gets or sets the flux type
    /// </summary>
    public PercentageFluxTypes FluxType { get; set; }
}
