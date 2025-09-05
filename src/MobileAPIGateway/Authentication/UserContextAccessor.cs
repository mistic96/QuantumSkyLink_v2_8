using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using MobileAPIGateway.Models;
using MobileAPIGateway.Models.Auth;

namespace MobileAPIGateway.Authentication;

/// <summary>
/// Service for accessing the current user context
/// </summary>
public class UserContextAccessor : IUserContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LogtoAuthenticationOptions _authOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContextAccessor"/> class
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor</param>
    /// <param name="authOptions">The authentication options</param>
    public UserContextAccessor(
        IHttpContextAccessor httpContextAccessor,
        IOptions<LogtoAuthenticationOptions> authOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _authOptions = authOptions.Value;
    }

    /// <summary>
    /// Gets the current user context
    /// </summary>
    /// <returns>The user context</returns>
    public UserContext GetUserContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null || !httpContext.User.Identity?.IsAuthenticated == true)
        {
            return new UserContext();
        }

        var claims = httpContext.User.Claims.ToList();
        
        var userContext = new UserContext
        {
            UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            Email = httpContext.User.FindFirstValue(_authOptions.EmailClaimType) ?? string.Empty,
            Roles = httpContext.User.FindAll(_authOptions.RoleClaimType).Select(c => c.Value),
            Claims = claims
        };

        return userContext;
    }
}

/// <summary>
/// Extension methods for user context services
/// </summary>
public static class UserContextServiceExtensions
{
    /// <summary>
    /// Adds user context services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContextAccessor, UserContextAccessor>();
        services.AddScoped<UserContextAccessor>(); // Register concrete class for direct injection
        
        return services;
    }
}
