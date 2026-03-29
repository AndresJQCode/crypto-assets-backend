using System.Reflection;
using Api.Application.Services;
using Api.Infrastructure.HealthChecks;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.PermissionAggregate;
using Domain.Interfaces;
using FluentValidation;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Api.Extensions;

internal static class Extensions
{
    private static readonly string[] ApiReadyTags = ["api", "ready"];
    private static readonly string[] DbReadyTags = ["db", "ready"];
    private static readonly string[] CacheReadyTags = ["cache", "ready"];
    private static readonly string[] IdentityReadyTags = ["identity", "ready"];
    private static readonly string[] ConfigReadyTags = ["config", "ready"];

    public static IHealthChecksBuilder AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddNpgsqlDataSource(configuration.GetRequiredConnectionString("DB"));

        // Configurar health checks simplificados - métricas detalladas van a Prometheus
        return services.AddHealthChecks()
            // Health check básico de la aplicación
            .AddCheck("self", () => HealthCheckResult.Healthy("API funcionando"), tags: ApiReadyTags)

            // Health check de PostgreSQL (solo conectividad)
            .AddNpgSql(
                name: "database",
                tags: DbReadyTags,
                timeout: TimeSpan.FromSeconds(5))

            // Health checks de servicios críticos (solo disponibilidad)
            .AddCheck<MemoryCacheHealthCheck>(
                name: "cache",
                tags: CacheReadyTags,
                timeout: TimeSpan.FromSeconds(3))

            .AddCheck<IdentityHealthCheck>(
                name: "identity",
                tags: IdentityReadyTags,
                timeout: TimeSpan.FromSeconds(3))

            .AddCheck<EmailServiceHealthCheck>(
                name: "email-config",
                tags: ConfigReadyTags,
                timeout: TimeSpan.FromSeconds(2));
    }

    public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApiContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetRequiredConnectionString("DB"),
                npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(ApiContext).Assembly.FullName);
                        // Optimizaciones de PostgreSQL
                        npgsqlOptions.CommandTimeout(30);
                    }
            );

            // Configuraciones de rendimiento

            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
            options.EnableServiceProviderCaching();
            options.EnableThreadSafetyChecks();
        });

        return services;
    }

    public static IServiceCollection AddHttpFactoryServices(this IServiceCollection services, IConfiguration configuration)
    {


        return services;
    }

    public static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration);

        // Registrar validador de configuración JWT
        // ValidateOnStart() garantiza que la validación se ejecute al iniciar la aplicación
        // La aplicación fallará inmediatamente si la configuración JWT es inválida
        services.AddSingleton<IValidateOptions<AppSettings>, JwtConfigurationValidator>();
        services.AddOptions<AppSettings>().ValidateOnStart();

        // ApiBehaviorOptions no es necesario para Minimal APIs
        // La validación se maneja directamente en los endpoints


        return services;
    }

    public static IServiceCollection AddPermissionModule(this IServiceCollection services)
    {
        // Configurar opciones del caché de permisos
        services.Configure<PermissionCacheOptions>(options =>
        {
            options.AbsoluteExpiration = TimeSpan.FromMinutes(15);
            options.SlidingExpiration = TimeSpan.FromMinutes(5);
            options.Priority = CacheItemPriority.Normal;
            options.MaxCacheSize = 1000;
            options.EnableDetailedLogging = true;
        });

        services.AddSingleton<IPermissionCacheService, PermissionCacheService>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionRoleRepository, PermissionRoleRepository>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IUserPermissionService, UserPermissionService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IQueryOptimizationService, QueryOptimizationService>();

        return services;
    }

    /// <summary>
    /// Agregar middleware de permisos con métricas
    /// </summary>
    public static IServiceCollection AddPermissionMiddlewareModule(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddPermissionMiddleware();
        return services;
    }

    public static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        // Registrar todos los validadores de FluentValidation del assembly actual
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}

