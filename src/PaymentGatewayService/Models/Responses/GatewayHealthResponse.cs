namespace PaymentGatewayService.Models.Responses;

/// <summary>
/// Response model for gateway health status
/// </summary>
public class GatewayHealthResponse
{
    /// <summary>
    /// Gets or sets the overall health status
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets the health check message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the individual gateway health statuses
    /// </summary>
    public IEnumerable<GatewayHealthStatus> GatewayStatuses { get; set; } = new List<GatewayHealthStatus>();

    /// <summary>
    /// Gets or sets when the health check was performed
    /// </summary>
    public DateTime CheckedAt { get; set; }
}

/// <summary>
/// Individual gateway health status
/// </summary>
public class GatewayHealthStatus
{
    /// <summary>
    /// Gets or sets the gateway ID
    /// </summary>
    public Guid GatewayId { get; set; }

    /// <summary>
    /// Gets or sets the gateway name
    /// </summary>
    public string GatewayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the gateway is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets the health status message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>
    /// Gets or sets when this gateway was last checked
    /// </summary>
    public DateTime LastChecked { get; set; }
}
