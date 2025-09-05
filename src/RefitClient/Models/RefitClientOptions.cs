using System;

namespace RefitClient.Models;

/// <summary>
/// Configuration options for Refit clients.
/// </summary>
public class RefitClientOptions
{
    /// <summary>
    /// Gets or sets the base URL for the API.
    /// If not specified, service discovery will be used with the service name.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the timeout for HTTP requests.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether to use authentication.
    /// </summary>
    public bool UseAuthentication { get; set; }

    /// <summary>
    /// Gets or sets the authentication scheme to use when UseAuthentication is true.
    /// Default is "Bearer".
    /// </summary>
    public string AuthenticationScheme { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets whether to use standard resilience policies.
    /// Default is true.
    /// </summary>
    public bool UseResiliencePolicies { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use service discovery.
    /// Default is true.
    /// </summary>
    public bool UseServiceDiscovery { get; set; } = true;

    /// <summary>
    /// Gets or sets additional headers to include with every request.
    /// </summary>
    public Dictionary<string, string>? DefaultHeaders { get; set; }
}
