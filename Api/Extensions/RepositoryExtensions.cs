using Domain.AggregatesModel.OrderAggregate;
using Domain.AggregatesModel.SystemConfigurationAggregate;
using Domain.AggregatesModel.TenantAggregate;
using Domain.AggregatesModel.TradingOrderAggregate;
using Domain.Interfaces;
using Domain.SeedWork;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.Trading;

namespace Api.Extensions;

internal static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Registrar servicios de auditoría
        services.AddScoped<IAuditTrail, AuditTrailService>();

        // Registrar repositorios específicos con métodos adicionales
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITradingOrderRepository, TradingOrderRepository>();

        // Registrar servicios de trading
        services.AddScoped<IBybitService, BybitService>();

        return services;
    }
}
