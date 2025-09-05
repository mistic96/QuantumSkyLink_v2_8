using MobileAPIGateway.Authentication;

namespace MobileAPIGateway.Middleware;

/// <summary>
/// Middleware for extracting user context from authenticated requests
/// </summary>
public class UserContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserContextMiddleware> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="UserContextMiddleware"/> class
    /// </summary>
    /// <param name="next">Request delegate</param>
    /// <param name="logger">Logger</param>
    public UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    /// <summary>
    /// Invokes the middleware
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <param name="userContextAccessor">User context accessor</param>
    /// <returns>Task</returns>
    public async Task InvokeAsync(HttpContext context, UserContextAccessor userContextAccessor)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                var userContext = userContextAccessor.GetUserContext();
                
                // Store the user context in the HttpContext.Items collection for easy access
                context.Items["UserContext"] = userContext;
                
                _logger.LogInformation("User context extracted for user {Email}", userContext.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting user context");
            }
        }
        
        await _next(context);
    }
}

/// <summary>
/// User context middleware extensions
/// </summary>
public static class UserContextMiddlewareExtensions
{
    /// <summary>
    /// Uses the user context middleware
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseUserContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserContextMiddleware>();
    }
}
