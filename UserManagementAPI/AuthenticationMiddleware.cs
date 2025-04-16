// AuthenticationMiddleware.cs
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    // Hardcoded valid token for demonstration purposes.
    private const string ValidToken = "mysecrettoken";
    
    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the Authorization header exists
        if (!context.Request.Headers.TryGetValue("Authorization", out var token))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Unauthorized: Token is missing.");
            return;
        }

        // Expecting a token formatted as "Bearer <token-value>"
        var tokenValue = token.FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(tokenValue) || tokenValue != ValidToken)
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Unauthorized: Invalid token.");
            return;
        }
        
        // If token is valid, continue processing the request
        await _next(context);
    }
}