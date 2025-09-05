namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// Request model for accepting terms and conditions
/// </summary>
public class AcceptTermsRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Terms version accepted
    /// </summary>
    public string TermsVersion { get; set; } = string.Empty;

    /// <summary>
    /// Date when terms were accepted
    /// </summary>
    public DateTime AcceptedDate { get; set; }

    /// <summary>
    /// IP address from which terms were accepted
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}
