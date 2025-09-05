using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Authentication;
using MobileAPIGateway.Models;

namespace MobileAPIGateway.Controllers.Base;

/// <summary>
/// Base controller for all API controllers
/// </summary>
[ApiController]
[Route("[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private readonly UserContextAccessor _userContextAccessor;
    private UserContext? _userContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiControllerBase"/> class
    /// </summary>
    /// <param name="userContextAccessor">The user context accessor</param>
    protected ApiControllerBase(UserContextAccessor userContextAccessor)
    {
        _userContextAccessor = userContextAccessor;
    }

    /// <summary>
    /// Gets the current user context
    /// </summary>
    protected UserContext CurrentUser => _userContext ??= _userContextAccessor.GetUserContext();

    /// <summary>
    /// Gets the current user's email
    /// </summary>
    protected string UserEmail => CurrentUser.Email;

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="statusCode">The HTTP status code</param>
    /// <returns>An error response</returns>
    protected IActionResult ErrorResponse(string message, int statusCode = 400)
    {
        var response = new
        {
            error = new
            {
                message,
                statusCode,
                requestId = HttpContext.TraceIdentifier
            }
        };

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Creates a standardized success response
    /// </summary>
    /// <param name="data">The response data</param>
    /// <returns>A success response</returns>
    protected IActionResult SuccessResponse(object data)
    {
        var response = new
        {
            data,
            requestId = HttpContext.TraceIdentifier
        };

        return Ok(response);
    }

    /// <summary>
    /// Ensures the user is authenticated
    /// </summary>
    /// <returns>True if the user is authenticated, otherwise false</returns>
    protected bool EnsureAuthenticated()
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return false;
        }

        return true;
    }
}
