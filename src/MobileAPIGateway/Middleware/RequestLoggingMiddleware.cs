using System.Diagnostics;
using System.Text;

namespace MobileAPIGateway.Middleware;

/// <summary>
/// Request logging middleware
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class
    /// </summary>
    /// <param name="next">Request delegate</param>
    /// <param name="logger">Logger</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    /// <summary>
    /// Invokes the middleware
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>Task</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var requestBody = string.Empty;
        
        // Only log request body for specific content types
        if (request.ContentType != null && 
            (request.ContentType.Contains("application/json") || 
             request.ContentType.Contains("application/xml") || 
             request.ContentType.Contains("text/plain")))
        {
            request.EnableBuffering();
            
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        // Log request
        _logger.LogInformation(
            "Request: {Method} {Path} {QueryString} {RequestBody}",
            request.Method,
            request.Path,
            request.QueryString,
            requestBody);
        
        // Capture the original response body stream
        var originalBodyStream = context.Response.Body;
        
        // Create a new memory stream to capture the response body
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;
        
        try
        {
            // Call the next middleware
            await _next(context);
            
            // Reset the position of the response body stream
            responseBodyStream.Position = 0;
            
            // Read the response body
            var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            
            // Log response
            _logger.LogInformation(
                "Response: {StatusCode} {ResponseBody} (Elapsed: {ElapsedMilliseconds}ms)",
                context.Response.StatusCode,
                responseBody,
                stopwatch.ElapsedMilliseconds);
            
            // Reset the position of the response body stream
            responseBodyStream.Position = 0;
            
            // Copy the response body to the original stream
            await responseBodyStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            // Restore the original response body stream
            context.Response.Body = originalBodyStream;
        }
    }
}

/// <summary>
/// Request logging middleware extensions
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    /// <summary>
    /// Uses the request logging middleware
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
