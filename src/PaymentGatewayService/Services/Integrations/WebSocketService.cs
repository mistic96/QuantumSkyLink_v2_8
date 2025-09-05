using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// WebSocket connection management service
/// </summary>
public class WebSocketService : IWebSocketService, IDisposable
{
    private readonly ILogger<WebSocketService> _logger;
    private readonly ConcurrentDictionary<string, WebSocketConnection> _connections;

    public WebSocketService(ILogger<WebSocketService> logger)
    {
        _logger = logger;
        _connections = new ConcurrentDictionary<string, WebSocketConnection>();
    }

    /// <summary>
    /// Connects to a WebSocket endpoint
    /// </summary>
    public async Task<string> ConnectAsync(Uri uri, Action<string> messageHandler, CancellationToken cancellationToken = default)
    {
        var connectionId = Guid.NewGuid().ToString();
        var webSocket = new ClientWebSocket();

        try
        {
            _logger.LogInformation("Connecting to WebSocket: {Uri}", uri);
            await webSocket.ConnectAsync(uri, cancellationToken);

            var connection = new WebSocketConnection
            {
                Id = connectionId,
                WebSocket = webSocket,
                Uri = uri,
                MessageHandler = messageHandler,
                CancellationTokenSource = new CancellationTokenSource()
            };

            _connections[connectionId] = connection;

            // Start listening for messages
            _ = Task.Run(async () => await ListenForMessagesAsync(connection), connection.CancellationTokenSource.Token);

            _logger.LogInformation("WebSocket connected successfully. ConnectionId: {ConnectionId}", connectionId);
            return connectionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to WebSocket: {Uri}", uri);
            webSocket?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Sends a message through WebSocket
    /// </summary>
    public async Task SendMessageAsync(string connectionId, string message, CancellationToken cancellationToken = default)
    {
        if (!_connections.TryGetValue(connectionId, out var connection))
        {
            throw new InvalidOperationException($"Connection {connectionId} not found");
        }

        if (connection.WebSocket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException($"Connection {connectionId} is not open");
        }

        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(bytes);
            
            await connection.WebSocket.SendAsync(
                buffer,
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            _logger.LogDebug("Message sent on connection {ConnectionId}", connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message on connection {ConnectionId}", connectionId);
            throw;
        }
    }

    /// <summary>
    /// Disconnects from WebSocket
    /// </summary>
    public async Task DisconnectAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        if (!_connections.TryRemove(connectionId, out var connection))
        {
            return;
        }

        try
        {
            connection.CancellationTokenSource.Cancel();

            if (connection.WebSocket.State == WebSocketState.Open)
            {
                await connection.WebSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closing connection",
                    cancellationToken);
            }

            connection.WebSocket.Dispose();
            connection.CancellationTokenSource.Dispose();

            _logger.LogInformation("WebSocket disconnected. ConnectionId: {ConnectionId}", connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting WebSocket. ConnectionId: {ConnectionId}", connectionId);
        }
    }

    /// <summary>
    /// Checks if connection is alive
    /// </summary>
    public bool IsConnected(string connectionId)
    {
        if (!_connections.TryGetValue(connectionId, out var connection))
        {
            return false;
        }

        return connection.WebSocket.State == WebSocketState.Open;
    }

    /// <summary>
    /// Gets all active connections
    /// </summary>
    public IEnumerable<string> GetActiveConnections()
    {
        return _connections
            .Where(kvp => kvp.Value.WebSocket.State == WebSocketState.Open)
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Listens for incoming messages
    /// </summary>
    private async Task ListenForMessagesAsync(WebSocketConnection connection)
    {
        var buffer = new ArraySegment<byte>(new byte[4096]);
        var messageBuilder = new StringBuilder();

        try
        {
            while (connection.WebSocket.State == WebSocketState.Open && 
                   !connection.CancellationTokenSource.Token.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                messageBuilder.Clear();

                do
                {
                    result = await connection.WebSocket.ReceiveAsync(
                        buffer,
                        connection.CancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var text = Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, result.Count);
                        messageBuilder.Append(text);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("WebSocket close message received. ConnectionId: {ConnectionId}", connection.Id);
                        await DisconnectAsync(connection.Id);
                        return;
                    }
                } while (!result.EndOfMessage);

                if (messageBuilder.Length > 0)
                {
                    var message = messageBuilder.ToString();
                    _logger.LogDebug("Message received on connection {ConnectionId}: {MessageLength} bytes", 
                        connection.Id, message.Length);

                    try
                    {
                        connection.MessageHandler(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in message handler for connection {ConnectionId}", connection.Id);
                    }
                }
            }
        }
        catch (WebSocketException ex)
        {
            _logger.LogError(ex, "WebSocket error on connection {ConnectionId}", connection.Id);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("WebSocket listener cancelled for connection {ConnectionId}", connection.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in WebSocket listener for connection {ConnectionId}", connection.Id);
        }
        finally
        {
            await DisconnectAsync(connection.Id);
        }
    }

    /// <summary>
    /// Dispose of all connections
    /// </summary>
    public void Dispose()
    {
        var connectionIds = _connections.Keys.ToList();
        foreach (var connectionId in connectionIds)
        {
            DisconnectAsync(connectionId).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Internal class to track WebSocket connections
    /// </summary>
    private class WebSocketConnection
    {
        public string Id { get; set; } = string.Empty;
        public ClientWebSocket WebSocket { get; set; } = new();
        public Uri Uri { get; set; } = null!;
        public Action<string> MessageHandler { get; set; } = null!;
        public CancellationTokenSource CancellationTokenSource { get; set; } = new();
    }
}