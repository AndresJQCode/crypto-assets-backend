using System.Diagnostics;
using Api.Extensions;
using Api.Infrastructure.Metrics;
using Domain.Exceptions;
using MediatR;

namespace Api.Application.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = request.GetGenericTypeName();
        var requestType = typeof(TRequest).Name.Contains("Command", StringComparison.OrdinalIgnoreCase) ? "command" : "query";

        logger.LogInformation("Handling {RequestType} {RequestName} ({@Request})", requestType, requestName, request);

        var stopwatch = Stopwatch.StartNew();
        string status = "success";
        string errorType = "";

        try
        {
            var response = await next(cancellationToken);
            logger.LogInformation("{RequestType} {RequestName} handled - response: {@Response}", requestType, requestName, response);
            return response;
        }
        catch (Exception ex)
        {
            status = "error";
            errorType = ex.GetType().Name;

            // Las excepciones de negocio esperadas se registran como Warning, no como Error
            // ya que son errores de validación/negocio esperados, no errores del sistema
            if (ex is BadRequestException or NotFoundException or UnAuthorizedException or DomainException)
            {
                logger.LogWarning(ex, "Business exception in {RequestType} {RequestName}: {Message}",
                    requestType, requestName, ex.Message);
            }
            else
            {
                // Registrar error en métricas solo para excepciones inesperadas
                ApiMetrics.MediatRRequestErrors
                    .WithLabels(requestType, requestName, errorType)
                    .Inc();

                logger.LogError(ex, "Error handling {RequestType} {RequestName}", requestType, requestName);
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed.TotalSeconds;

            // Registrar duración
            ApiMetrics.MediatRRequestDuration
                .WithLabels(requestType, requestName)
                .Observe(duration);

            // Incrementar contador total
            ApiMetrics.MediatRRequestsTotal
                .WithLabels(requestType, requestName, status)
                .Inc();
        }
    }
}

