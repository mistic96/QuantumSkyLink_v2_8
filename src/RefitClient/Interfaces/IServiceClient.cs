namespace RefitClient.Interfaces;

/// <summary>
/// Marker interface for service clients.
/// Implement this interface and add Refit attributes to define your API endpoints.
/// </summary>
/// <example>
/// <code>
/// public interface IUserServiceClient : IServiceClient
/// {
///     [Get("/users")]
///     Task&lt;List&lt;User&gt;&gt; GetUsersAsync();
///     
///     [Get("/users/{id}")]
///     Task&lt;User&gt; GetUserAsync(int id);
///     
///     [Post("/users")]
///     Task&lt;User&gt; CreateUserAsync([Body] User user);
/// }
/// </code>
/// </example>
public interface IServiceClient
{
    // This is a marker interface that doesn't define any members.
    // It's used to constrain generic type parameters and for registration in DI.
}
