namespace MobileAPIGateway.Models.Compatibility.Auth;

/// <summary>
/// Extension methods for standard response messages
/// </summary>
public static class StandardResponseMessageExtensions
{
    /// <summary>
    /// Converts a string to a problem message response
    /// </summary>
    /// <param name="message">The message</param>
    /// <returns>The problem message response</returns>
    public static object AsProblemMessageResponse(this string message)
    {
        return new { error = message };
    }
}
