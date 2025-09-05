namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// Response model for MFA registration
/// </summary>
public class MfaRegistrationResponse
{
    /// <summary>
    /// QR code for MFA setup (base64 encoded)
    /// </summary>
    public string QrCode { get; set; } = string.Empty;

    /// <summary>
    /// Secret key for MFA
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Backup codes for MFA
    /// </summary>
    public List<string> BackupCodes { get; set; } = new();
}
