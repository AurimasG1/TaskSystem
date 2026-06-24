using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskSystem.Domain.Exceptions;

namespace TaskSystem.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
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
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        HttpStatusCode statusCode = ex switch
        {
            // Domain exceptions
            UserAlreadyExistsException => HttpStatusCode.BadRequest,
            InvalidCredentialsException => HttpStatusCode.BadRequest,
            UserNotFoundException => HttpStatusCode.NotFound,
            UzduotisNotFoundException => HttpStatusCode.NotFound,

            // Validation
            ValidationException => HttpStatusCode.BadRequest,

            // EF Core
            DbUpdateException => HttpStatusCode.Conflict,

            // Auth
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            ForbiddenAccessException => HttpStatusCode.Forbidden,

            // JSON / Model binding
            JsonException => HttpStatusCode.BadRequest,

            // Default
            _ => HttpStatusCode.InternalServerError,
        };

        var response = new
        {
            success = false,
            error = ex.Message,
            status = (int)statusCode,
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
