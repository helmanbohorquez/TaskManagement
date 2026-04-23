using System.Net;
using System.Text.Json;
using FluentValidation;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            await WriteError(context, HttpStatusCode.BadRequest, "ValidationError",
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
        }
        catch (NotFoundException ex)
        {
            await WriteError(context, HttpStatusCode.NotFound, "NotFound", ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteError(context, HttpStatusCode.Conflict, "Conflict", ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            await WriteError(context, HttpStatusCode.Unauthorized, "Unauthorized", ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteError(context, HttpStatusCode.BadRequest, "DomainError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteError(context, HttpStatusCode.InternalServerError, "ServerError", "Unexpected error.");
        }
    }

    private static Task WriteError(HttpContext context, HttpStatusCode status, string code, string message)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new { code, message },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return context.Response.WriteAsync(payload);
    }
}
