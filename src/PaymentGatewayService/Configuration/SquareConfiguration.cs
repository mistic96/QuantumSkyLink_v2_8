using System.Collections.Generic;

namespace PaymentGatewayService.Configuration;

/// <summary>
/// Strongly-typed configuration for Square integration. Bind from configuration section "Square".
/// Includes sandbox defaults where appropriate. Secrets should be provided via environment variables/user secrets in non-dev.
/// </summary>
public class SquareConfiguration
{
    /// <summary>
    /// Square application ID (sandbox or production).
    /// </summary>
    public required string ApplicationId { get; set; }

    /// <summary>
    /// Square access token (sandbox or production). Treat as secret.
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// Environment name: "sandbox" or "production". Defaults to "sandbox".
    /// </summary>
    public string Environment { get; set; } = "sandbox";

    /// <summary>
    /// Webhook signature verification key. Required for verifying incoming webhooks outside of local/dev testing.
    /// </summary>
    public string? WebhookSignatureKey { get; set; }

    /// <summary>
    /// Optional Square Location ID used for certain payment scenarios.
    /// </summary>
    public string? LocationId { get; set; }

    /// <summary>
    /// Supported digital wallets, defaults to Apple Pay and Google Pay.
    /// </summary>
    public List<string> SupportedDigitalWallets { get; set; } = new() { "APPLE_PAY", "GOOGLE_PAY" };

    /// <summary>
    /// Enable saving cards on file where applicable.
    /// </summary>
    public bool EnableCardOnFile { get; set; } = true;

    /// <summary>
    /// Default request timeout in seconds when calling Square APIs.
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;
}
