using System.Net;
using System.Text.Json;

namespace QuantumLedger.Hub.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            Success = false,
            Message = GetUserFriendlyMessage(exception),
            Error = exception.GetType().Name,
            // Include stack trace only in development
            StackTrace = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true 
                ? exception.StackTrace 
                : null
        };

        context.Response.StatusCode = GetStatusCode(exception);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        // Add specific exception types and their user-friendly messages
        return exception switch
        {
            ArgumentException _ => "Invalid input provided",
            UnauthorizedAccessException _ => "You are not authorized to perform this action",
            InvalidOperationException _ => "The requested operation is invalid",
            _ => "An unexpected error occurred"
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        // Map exception types to HTTP status codes
        return exception switch
        {
            ArgumentException _ => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException _ => (int)HttpStatusCode.Unauthorized,
            InvalidOperationException _ => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}

// Extension method to make it easier to add the middleware
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
