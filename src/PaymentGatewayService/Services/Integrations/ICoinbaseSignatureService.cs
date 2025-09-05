namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// Interface for Coinbase request signature generation using ECDSA
/// </summary>
public interface ICoinbaseSignatureService
{
    /// <summary>
    /// Generates a signature for a Coinbase API request
    /// </summary>
    /// <param name="method">HTTP method (GET, POST, etc.)</param>
    /// <param name="path">Request path without base URL</param>
    /// <param name="body">Request body (empty string for GET requests)</param>
    /// <param name="timestamp">Unix timestamp</param>
    /// <returns>Base64 encoded signature</returns>
    string GenerateSignature(string method, string path, string body, string timestamp);

    /// <summary>
    /// Generates JWT token for WebSocket authentication
    /// </summary>
    /// <returns>JWT token for WebSocket connection</returns>
    string GenerateWebSocketJWT();

    /// <summary>
    /// Validates that the signature service is properly configured
    /// </summary>
    /// <returns>True if configuration is valid</returns>
    bool ValidateConfiguration();
}