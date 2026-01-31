using System.Text.Json;
using HotelSearch.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace HotelSearch.Api.Middlewares;

/// <summary>
/// Global exception handling middleware for consistent error responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, IHostEnvironment environment, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _environment = environment;
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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            DuplicateHotelException => (StatusCodes.Status409Conflict, "Duplicate hotel"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation"),
            _ => (StatusCodes.Status500InternalServerError, "An error occurred while processing your request")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = _environment.IsDevelopment() ? exception.Message : SanitizeExceptionMessage(exception),
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        // Add stack trace in development mode
        if (_environment.IsDevelopment() && exception is not NotFoundException and not DuplicateHotelException)
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var options = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        return context.Response.WriteAsJsonAsync(problemDetails, options);
    }

    private static string SanitizeExceptionMessage(Exception exception)
    {
        return exception switch
        {
            NotFoundException => exception.Message,
            DuplicateHotelException => exception.Message,
            ArgumentException => exception.Message,
            _ => "An unexpected error occurred. Please try again later."
        };
    }
}