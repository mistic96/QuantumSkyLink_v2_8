namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// Interface for WebSocket connection management
/// </summary>
public interface IWebSocketService
{
    /// <summary>
    /// Connects to a WebSocket endpoint
    /// </summary>
    /// <param name="uri">WebSocket URI</param>
    /// <param name="messageHandler">Handler for incoming messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connection ID</returns>
    Task<string> ConnectAsync(Uri uri, Action<string> messageHandler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message through WebSocket
    /// </summary>
    /// <param name="connectionId">Connection ID</param>
    /// <param name="message">Message to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendMessageAsync(string connectionId, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from WebSocket
    /// </summary>
    /// <param name="connectionId">Connection ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DisconnectAsync(string connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if connection is alive
    /// </summary>
    /// <param name="connectionId">Connection ID</param>
    /// <returns>True if connection is active</returns>
    bool IsConnected(string connectionId);

    /// <summary>
    /// Gets all active connections
    /// </summary>
    /// <returns>List of active connection IDs</returns>
    IEnumerable<string> GetActiveConnections();
}