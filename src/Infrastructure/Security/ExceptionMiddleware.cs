using System.Text.Json;
using BankMore.Domain;

namespace BankMore.Infrastructure.Security;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var body = new
            {
                message = ex.Message,
                type = ex.ErrorType
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
        catch (Exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
