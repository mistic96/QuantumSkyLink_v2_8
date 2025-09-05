using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models;

namespace MobileAPIGateway.Controllers;

/// <summary>
/// Base controller for all controllers in the application
/// </summary>
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current user context
    /// </summary>
    protected UserContext CurrentUser
    {
        get
        {
            if (HttpContext.Items.TryGetValue("UserContext", out var userContext))
            {
                return (UserContext)userContext;
            }
            
            return new UserContext();
        }
    }
    
    /// <summary>
    /// Gets the current user's email
    /// </summary>
    protected string CurrentUserEmail => CurrentUser.Email;
    
    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    protected string CurrentUserId => CurrentUser.UserId;
    
    /// <summary>
    /// Gets a value indicating whether the current user is authenticated
    /// </summary>
    protected bool IsAuthenticated => CurrentUser.IsAuthenticated;
    
    /// <summary>
    /// Checks if the current user has the specified role
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if the user has the role, otherwise false</returns>
    protected bool UserHasRole(string role) => CurrentUser.HasRole(role);
    
    /// <summary>
    /// Gets a claim value for the current user
    /// </summary>
    /// <param name="claimType">The claim type</param>
    /// <returns>The claim value, or null if not found</returns>
    protected string? GetUserClaimValue(string claimType) => CurrentUser.GetClaimValue(claimType);
}
