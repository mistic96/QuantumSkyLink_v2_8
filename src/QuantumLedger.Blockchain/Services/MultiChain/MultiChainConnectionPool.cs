using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuantumLedger.Blockchain.Services.MultiChain
{
    /// <summary>
    /// Configuration options for MultiChain connection pooling.
    /// </summary>
    public class MultiChainConnectionOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of connections per endpoint.
        /// </summary>
        public int MaxConnectionsPerEndpoint { get; set; } = 10;

        /// <summary>
        /// Gets or sets the connection timeout in seconds.
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the request timeout in seconds.
        /// </summary>
        public int RequestTimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the connection idle timeout in minutes.
        /// </summary>
        public int ConnectionIdleTimeoutMinutes { get; set; } = 5;

        /// <summary>
        /// Gets or sets whether to enable connection pooling.
        /// </summary>
        public bool EnableConnectionPooling { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of retries for failed requests.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets the retry delay in milliseconds.
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 1000;
    }

    /// <summary>
    /// Manages HTTP connection pooling for MultiChain RPC calls with optimized performance.
    /// </summary>
    public class MultiChainConnectionPool : IDisposable
    {
        private readonly ILogger<MultiChainConnectionPool> _logger;
        private readonly MultiChainConnectionOptions _options;
        private readonly ConcurrentDictionary<string, HttpClient> _httpClients;
        private readonly Timer _cleanupTimer;
        private readonly SemaphoreSlim _connectionSemaphore;
        private bool _disposed;

        // Connection statistics
        private long _totalConnections;
        private long _activeConnections;
        private long _totalRequests;
        private long _failedRequests;

        /// <summary>
        /// Initializes a new instance of the MultiChainConnectionPool class.
        /// </summary>
        /// <param name="options">Connection pool options.</param>
        /// <param name="logger">Logger instance.</param>
        public MultiChainConnectionPool(
            IOptions<MultiChainConnectionOptions> options,
            ILogger<MultiChainConnectionPool> logger)
        {
            _options = options?.Value ?? new MultiChainConnectionOptions();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClients = new ConcurrentDictionary<string, HttpClient>();
            _connectionSemaphore = new SemaphoreSlim(_options.MaxConnectionsPerEndpoint, _options.MaxConnectionsPerEndpoint);

            // Set up cleanup timer to run every minute
            _cleanupTimer = new Timer(CleanupIdleConnections, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            _logger.LogInformation("MultiChain connection pool initialized with max {MaxConnections} connections per endpoint",
                _options.MaxConnectionsPerEndpoint);
        }

        /// <summary>
        /// Gets an optimized HTTP client for the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The MultiChain endpoint URL.</param>
        /// <param name="rpcUser">RPC username.</param>
        /// <param name="rpcPassword">RPC password.</param>
        /// <returns>An optimized HTTP client.</returns>
        public async Task<HttpClient> GetHttpClientAsync(string endpoint, string rpcUser, string rpcPassword)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

            var clientKey = $"{endpoint}:{rpcUser}";

            // Try to get existing client
            if (_httpClients.TryGetValue(clientKey, out var existingClient))
            {
                Interlocked.Increment(ref _totalRequests);
                return existingClient;
            }

            // Wait for available connection slot
            await _connectionSemaphore.WaitAsync();

            try
            {
                // Double-check pattern
                if (_httpClients.TryGetValue(clientKey, out existingClient))
                {
                    Interlocked.Increment(ref _totalRequests);
                    return existingClient;
                }

                // Create new optimized HTTP client
                var httpClient = CreateOptimizedHttpClient(endpoint, rpcUser, rpcPassword);
                _httpClients.TryAdd(clientKey, httpClient);

                Interlocked.Increment(ref _totalConnections);
                Interlocked.Increment(ref _activeConnections);
                Interlocked.Increment(ref _totalRequests);

                _logger.LogDebug("Created new HTTP client for endpoint: {Endpoint}", endpoint);

                return httpClient;
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        /// <summary>
        /// Creates an optimized HTTP client with performance settings.
        /// </summary>
        /// <param name="endpoint">The endpoint URL.</param>
        /// <param name="rpcUser">RPC username.</param>
        /// <param name="rpcPassword">RPC password.</param>
        /// <returns>Optimized HTTP client.</returns>
        private HttpClient CreateOptimizedHttpClient(string endpoint, string rpcUser, string rpcPassword)
        {
            var handler = new SocketsHttpHandler
            {
                // Connection pooling settings
                PooledConnectionLifetime = TimeSpan.FromMinutes(_options.ConnectionIdleTimeoutMinutes),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(_options.ConnectionIdleTimeoutMinutes),
                MaxConnectionsPerServer = _options.MaxConnectionsPerEndpoint,

                // Performance optimizations
                UseCookies = false, // Disable cookies for better performance
                UseProxy = false,   // Disable proxy for better performance
                
                // Timeout settings
                ConnectTimeout = TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds),
                
                // Keep-alive settings
                EnableMultipleHttp2Connections = true,
            };

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(endpoint),
                Timeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds)
            };

            // Set up authentication
            var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{rpcUser}:{rpcPassword}"));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            // Performance headers
            httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "QuantumLedger-MultiChain-Client/1.0");

            return httpClient;
        }

        /// <summary>
        /// Executes an HTTP request with retry logic and connection management.
        /// </summary>
        /// <param name="endpoint">The endpoint URL.</param>
        /// <param name="rpcUser">RPC username.</param>
        /// <param name="rpcPassword">RPC password.</param>
        /// <param name="requestFunc">Function to execute the HTTP request.</param>
        /// <returns>The HTTP response.</returns>
        public async Task<HttpResponseMessage> ExecuteRequestAsync(
            string endpoint,
            string rpcUser,
            string rpcPassword,
            Func<HttpClient, Task<HttpResponseMessage>> requestFunc)
        {
            var retryCount = 0;
            Exception lastException = null;

            while (retryCount <= _options.MaxRetries)
            {
                try
                {
                    var httpClient = await GetHttpClientAsync(endpoint, rpcUser, rpcPassword);
                    var response = await requestFunc(httpClient);

                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }

                    // Log non-success status codes
                    _logger.LogWarning("HTTP request failed with status code: {StatusCode}, attempt {Attempt}",
                        response.StatusCode, retryCount + 1);

                    if (retryCount == _options.MaxRetries)
                    {
                        Interlocked.Increment(ref _failedRequests);
                        return response; // Return the last response even if not successful
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "HTTP request failed on attempt {Attempt}: {Message}",
                        retryCount + 1, ex.Message);

                    if (retryCount == _options.MaxRetries)
                    {
                        Interlocked.Increment(ref _failedRequests);
                        throw;
                    }
                }

                retryCount++;
                if (retryCount <= _options.MaxRetries)
                {
                    var delay = TimeSpan.FromMilliseconds(_options.RetryDelayMilliseconds * retryCount);
                    await Task.Delay(delay);
                }
            }

            // This should never be reached, but just in case
            Interlocked.Increment(ref _failedRequests);
            throw lastException ?? new InvalidOperationException("Request failed after all retries");
        }

        /// <summary>
        /// Gets connection pool statistics.
        /// </summary>
        /// <returns>Connection pool statistics.</returns>
        public ConnectionPoolStatistics GetStatistics()
        {
            return new ConnectionPoolStatistics
            {
                TotalConnections = _totalConnections,
                ActiveConnections = _activeConnections,
                TotalRequests = _totalRequests,
                FailedRequests = _failedRequests,
                SuccessRate = _totalRequests > 0 ? (double)(_totalRequests - _failedRequests) / _totalRequests * 100 : 0,
                PooledClients = _httpClients.Count
            };
        }

        /// <summary>
        /// Cleanup timer callback to remove idle connections.
        /// </summary>
        /// <param name="state">Timer state (unused).</param>
        private void CleanupIdleConnections(object state)
        {
            try
            {
                var stats = GetStatistics();
                _logger.LogDebug("Connection pool stats - Active: {Active}, Total: {Total}, Success Rate: {SuccessRate:F2}%",
                    stats.ActiveConnections, stats.TotalConnections, stats.SuccessRate);

                // Log warning if success rate is low
                if (stats.SuccessRate < 95 && stats.TotalRequests > 10)
                {
                    _logger.LogWarning("Connection pool success rate is low: {SuccessRate:F2}%", stats.SuccessRate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during connection pool cleanup");
            }
        }

        /// <summary>
        /// Disposes the connection pool and all HTTP clients.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _cleanupTimer?.Dispose();
            _connectionSemaphore?.Dispose();

            foreach (var client in _httpClients.Values)
            {
                client?.Dispose();
            }

            _httpClients.Clear();
            _disposed = true;

            _logger.LogInformation("MultiChain connection pool disposed");
        }
    }

    /// <summary>
    /// Represents connection pool statistics.
    /// </summary>
    public class ConnectionPoolStatistics
    {
        /// <summary>
        /// Gets or sets the total number of connections created.
        /// </summary>
        public long TotalConnections { get; set; }

        /// <summary>
        /// Gets or sets the number of currently active connections.
        /// </summary>
        public long ActiveConnections { get; set; }

        /// <summary>
        /// Gets or sets the total number of requests made.
        /// </summary>
        public long TotalRequests { get; set; }

        /// <summary>
        /// Gets or sets the number of failed requests.
        /// </summary>
        public long FailedRequests { get; set; }

        /// <summary>
        /// Gets the success rate as a percentage.
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Gets or sets the number of pooled HTTP clients.
        /// </summary>
        public int PooledClients { get; set; }
    }
}
