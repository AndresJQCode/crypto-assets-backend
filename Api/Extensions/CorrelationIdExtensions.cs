using Api.Infrastructure.Middlewares;

namespace Api.Extensions;

/// <summary>
/// Extensiones para configurar el middleware de Correlation ID
/// </summary>
internal static class CorrelationIdExtensions
{
    /// <summary>
    /// Agregar el middleware de Correlation ID al pipeline.
    /// Debe agregarse temprano en el pipeline para que todos los logs lo incluyan.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
