using System.Threading.Tasks;

namespace RefitClient.Interfaces;

/// <summary>
/// Defines a generic API client interface for making HTTP requests.
/// </summary>
/// <typeparam name="T">The type of the service client interface.</typeparam>
public interface IApiClient<T> where T : class
{
    /// <summary>
    /// Sends a GET request to the specified endpoint.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoint">The API endpoint.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    Task<TResponse> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request to the specified endpoint with the provided data.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request data.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoint">The API endpoint.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request to the specified endpoint with the provided data.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request data.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoint">The API endpoint.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request to the specified endpoint.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoint">The API endpoint.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    Task<TResponse> DeleteAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);
}
