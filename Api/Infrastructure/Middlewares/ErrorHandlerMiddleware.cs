using System.Net;
using System.Text.Json;
using Api.Constants;
using Domain.Exceptions;

namespace Api.Infrastructure.Middlewares;

internal sealed class ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = HeaderConstants.ContentType.ApplicationJson;

            var statusCode = error switch
            {
                BadRequestException or DomainException => HttpStatusCode.BadRequest,
                UnAuthorizedException => HttpStatusCode.Unauthorized,
                FileNotFoundException or NotFoundException or KeyNotFoundException => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };

            response.StatusCode = (int)statusCode;

            // Logging estructurado según el tipo de error
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                // Errores no manejados - log con nivel Error incluyendo stack trace
                logger.LogError(
                    error,
                    "Unhandled exception: {ErrorType} at {Method} {Path}",
                    error.GetType().Name,
                    requestMethod,
                    requestPath);
            }
            else
            {
                // Errores de dominio/validación - log con nivel Warning sin stack trace
                logger.LogWarning(
                    "Handled exception: {ErrorType} at {Method} {Path} - {Message}",
                    error.GetType().Name,
                    requestMethod,
                    requestPath,
                    error.Message);
            }

            var result = JsonSerializer.Serialize(new { message = error.Message });
            await response.WriteAsync(result);
        }
    }
}
