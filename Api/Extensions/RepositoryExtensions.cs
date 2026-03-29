using Domain.SeedWork;
using Infrastructure.Services;

namespace Api.Extensions;

internal static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Registrar servicios de auditoría
        services.AddScoped<IAuditTrail, AuditTrailService>();

        return services;
    }
}
