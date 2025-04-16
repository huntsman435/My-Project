// ErrorHandlingMiddleware.cs
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    
    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Process the request
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        
        var errorResponse = new { error = "Internal server error.", details = exception.Message };
        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        
        return context.Response.WriteAsync(jsonResponse);
    }
}