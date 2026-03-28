using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Constants;
using Api.Infrastructure.Middlewares;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.UserAggregate;
using Domain.Interfaces;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.ServiceBus;
using Infrastructure.ServiceBus.Interfaces;
using Infrastructure.Services.Loggers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Exceptions;

namespace Api.Extensions;


internal static class CommonExtensions
{
    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        // Configurar opciones JSON para Minimal APIs
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        builder.Services.AddDefaultSerilog(builder.Configuration);

        builder.Host.UseSerilog();

        // Default health checks assume the event bus and self health checks
        builder.Services.AddDefaultHealthChecks(builder.Configuration);

        builder.Services.AddDefaultAuthentication(builder.Configuration);

        builder.Services.AddDefaultOpenApi(builder.Configuration);

        // Add the accessor
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddHttpClients(builder.Configuration);

        // builder.Services.AddRepositories(); // Moved to RepositoryExtensions

        builder.Services.AddServiceBusHandlers();

        return builder;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<HttpClientLoggerHandler>();

        return services;
    }

    public static IServiceCollection AddServiceBusHandlers(this IServiceCollection services)
    {
        var handlerTypes = Assembly.GetExecutingAssembly()
                           .GetTypes()
                           .Where(t => typeof(IMessageHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var handlerType in handlerTypes)
        {
            services.AddTransient(handlerType);
        }

        return services;
    }

    public static WebApplication UseServiceBusHandlers(this WebApplication app, IConfiguration configuration)
    {
        var handlerTypes = Assembly.GetExecutingAssembly()
                           .GetTypes()
                           .Where(t => typeof(IMessageHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var handlerType in handlerTypes)
        {
            using var scope = app.Services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService(handlerType) as IMessageHandler;
            using var listener = new ServiceBusTopicListener(configuration["ServiceBus:ConnectionStrings"]!, handler!);
            listener.StartProcessing();
        }

        return app;
    }

    public static WebApplication UseServiceDefaults(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        var jwtSection = app.Configuration.GetSection("JwtSettings");

        if (jwtSection.Exists())
        {
            // We have to add the auth middleware to the pipeline here
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.MapDefaultHealthChecks();

        app.UseMiddleware<ErrorHandlerMiddleware>();

        app.UseServiceBusHandlers(app.Configuration);

        return app;
    }

    public static async Task<bool> CheckHealthAsync(this WebApplication app)
    {
        app.Logger.LogInformation("Running health checks...");

        // Do a health check on startup, this will throw an exception if any of the checks fail
        var report = await app.Services.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        if (report.Status == HealthStatus.Unhealthy)
        {
            app.Logger.LogCritical("Health checks failed!");
            foreach (var entry in report.Entries)
            {
                if (entry.Value.Status == HealthStatus.Unhealthy)
                {
                    app.Logger.LogCritical("{Check}: {Status}", entry.Key, entry.Value.Status);
                }
            }

            return false;
        }

        return true;
    }

    public static void AddDefaultSerilog(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
                 .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                 .ReadFrom.Configuration(configuration)
                 .CreateLogger();
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<IIdentityService, IdentityService>();

        return services;
    }

    public static IServiceCollection AddDefaultOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        var openApi = configuration.GetSection("OpenApi");

        if (!openApi.Exists())
        {
            return services;
        }

        services.AddEndpointsApiExplorer();

        return services.AddOpenApi(configureOptions: opt =>
        {
            opt.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new()
                {
                    Title = openApi.GetRequiredValue("Document:Title"),
                    Version = openApi.GetRequiredValue("Document:Version"),
                    Description = openApi.GetRequiredValue("Document:Description"),
                };
                return Task.CompletedTask;
            });
        });
    }

    public static IServiceCollection AddDefaultAuthentication(this IServiceCollection services, IConfiguration configuration)
    {

        var jwtSection = configuration.GetSection("JwtSettings");

        if (!jwtSection.Exists())
        {
            // No JWT settings section, so no authentication
            return services;
        }

        // Configurar la autenticación JWT
        var secretKey = jwtSection.GetRequiredValue("SecretKey");
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection.GetRequiredValue("Issuer"),
                ValidAudience = jwtSection.GetRequiredValue("Audience"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            // Agregar validaciones adicionales
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = (context) =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogError("JWT Authentication failed: {Error}", context.Exception?.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = async (context) =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                    var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (string.IsNullOrEmpty(userId))
                    {
                        logger.LogWarning("No user ID found in JWT token");
                        context.Fail("Unauthorized");
                        return;
                    }

                    logger.LogInformation("Validating JWT for user ID: {UserId}", userId);

                    var user = await userManager.FindByIdAsync(userId);

                    if (user == null)
                    {
                        logger.LogWarning("User not found for ID: {UserId}", userId);
                        context.Fail("Unauthorized");
                        return;
                    }

                    // validar que el token esté en bd
                    var jwtToken = context.SecurityToken as Microsoft.IdentityModel.JsonWebTokens.JsonWebToken;
                    var tokenInDb = await userManager.GetAuthenticationTokenAsync(user, AppConstants.Authentication.DefaultProvider, AppConstants.Authentication.AccessTokenName);

                    logger.LogInformation("Checking token for user {UserId} with provider '{Provider}': {HasToken}",
                        userId, AppConstants.Authentication.DefaultProvider, !string.IsNullOrEmpty(tokenInDb));

                    if (jwtToken?.EncodedToken != tokenInDb)
                    {
                        logger.LogWarning("Token mismatch for user ID: {UserId}. Expected: {Expected}, Found: {Found}",
                            userId, jwtToken?.EncodedToken, tokenInDb);
                        context.Fail("Unauthorized");
                        return;
                    }

                    logger.LogInformation("JWT validation successful for user ID: {UserId}", userId);

                    // Agregar roles dinámicamente desde la base de datos
                    var userRoles = await userManager.GetRolesAsync(user);
                    var identity = context.Principal?.Identity as ClaimsIdentity;

                    if (identity != null)
                    {
                        // Remover roles existentes (si los hay) y agregar los actuales
                        var existingRoleClaims = identity.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
                        foreach (var claim in existingRoleClaims)
                        {
                            identity.RemoveClaim(claim);
                        }
                    }

                    // Agregar roles actuales desde la base de datos
                    if (identity != null)
                    {
                        foreach (var role in userRoles)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, role));
                        }
                    }
                }
            };
        });

        services.AddAuthorization();
        return services;
    }

    public static IHealthChecksBuilder AddDefaultHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        // Delegar a la implementación completa de health checks en Extensions.cs
        return services.AddCustomHealthChecks(configuration);
    }

    public static void MapDefaultHealthChecks(this IEndpointRouteBuilder routes)
    {
        // Health check completo con información detallada
        routes.MapHealthChecks("/health", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        }).WithTags("Health")
          .WithSummary("Health Check completo")
          .WithDescription("Verifica el estado de todos los componentes del sistema");

        // Endpoint de health check simple (para balanceadores de carga)
        routes.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("api"),
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = HeaderConstants.ContentType.ApplicationJson;
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow
                });
                await context.Response.WriteAsync(result);
            }
        }).WithTags("Health")
          .WithSummary("Liveness Check")
          .WithDescription("Verifica si la API está viva (para Kubernetes liveness probe)");

        // Endpoint de readiness (verifica si está listo para recibir tráfico)
        routes.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        }).WithTags("Health")
          .WithSummary("Readiness Check")
          .WithDescription("Verifica si la API está lista para recibir tráfico (para Kubernetes readiness probe)");

        // Endpoint de health check de base de datos
        routes.MapHealthChecks("/health/db", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("db"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        }).WithTags("Health")
          .WithSummary("Database Health Check")
          .WithDescription("Verifica el estado de la base de datos");

        // Mantener compatibilidad con endpoints antiguos
        routes.MapHealthChecks("/hc", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        routes.MapHealthChecks("/liveness", new HealthCheckOptions
        {
            Predicate = r => r.Name.Contains("self", StringComparison.OrdinalIgnoreCase)
        });
    }
}

