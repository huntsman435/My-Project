// LoggingMiddleware.cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;
    
    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Log details before processing the request
        var method = context.Request.Method;
        var path = context.Request.Path;
        _logger.LogInformation($"Incoming Request: {method} {path}");
        
        await _next(context);
        
        // Log details after the response has been generated
        var statusCode = context.Response.StatusCode;
        _logger.LogInformation($"Outgoing Response: {statusCode}");
    }
}