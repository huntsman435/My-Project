using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ----------------- Middleware Pipeline Setup ----------------- //

// 1. Global Error-Handling Middleware: Catches exceptions and returns a JSON error response.
app.UseMiddleware<ErrorHandlingMiddleware>();

// 2. Authentication Middleware: Validates that requests include a valid token.
app.UseMiddleware<AuthenticationMiddleware>();

// 3. Logging Middleware: Logs incoming requests and outgoing responses.
app.UseMiddleware<LoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ----------------- Sample Endpoint and Existing Code ----------------- //
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();


// ----------------- Middleware Classes ----------------- //

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
            // Continue processing the request.
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the error to the console for debugging purposes.
            Console.WriteLine($"[DEBUG] Exception caught in ErrorHandlingMiddleware: {ex.Message}");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        
        var errorResponse = new { error = "Internal server error.", details = ex.Message };
        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        return context.Response.WriteAsync(jsonResponse);
    }
}

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    // For demo purposes, we use a hardcoded valid token.
    private const string ValidToken = "mysecrettoken";
    
    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    { 
        if (!context.Request.Headers.TryGetValue("Authorization", out var token))
        {
            Console.WriteLine("[DEBUG] Authorization header not found.");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Token is missing.");
            return;
        }
        
        // Expecting a token formatted as "Bearer <token>".
        var tokenValue = token.FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(tokenValue) || tokenValue != ValidToken)
        {
            Console.WriteLine($"[DEBUG] Invalid token provided: {tokenValue}");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Invalid token.");
            return;
        }
        
        // Token valid. Continuing request processing.
        await _next(context);
    }
}

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Log request details.
        var method = context.Request.Method;
        var path = context.Request.Path;
        Console.WriteLine($"[LOG] Incoming Request: {method} {path}");
        
        await _next(context);
        
        // Log response status.
        var statusCode = context.Response.StatusCode;
        Console.WriteLine($"[LOG] Outgoing Response: {statusCode}");
    }
}

// ----------------- Models ----------------- //

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class User
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }
}