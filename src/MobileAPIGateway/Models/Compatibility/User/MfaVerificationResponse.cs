namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// Response model for MFA verification
/// </summary>
public class MfaVerificationResponse
{
    /// <summary>
    /// Whether MFA verification was successful
    /// </summary>
    public bool Verified { get; set; }

    /// <summary>
    /// Verification message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
