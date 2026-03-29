using Api.Apis.AuthEndpoints;
using Api.Apis.DashboardEndpoints;
using Api.Apis.PermissionRolesEndpoints;
using Api.Apis.PermissionsEndpoints;
using Api.Apis.RolesEndpoints;
using Api.Apis.UsersEndpoints;
using Api.Application.Behaviors;
using Api.Extensions;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.EntityConfigurations;
using Infrastructure.Services.Email;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configurar servicios de email
builder.Services.AddTransient<IEmailSender<User>, InfobipEmailSender<User>>();
builder.Services.AddTransient<IEmailTemplateService, SimpleEmailTemplateService>();

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<ApiContext>()
    .AddDefaultTokenProviders();

builder.AddServiceDefaults();

builder.Services.AddDbContexts(builder.Configuration);
builder.Services.AddApplicationOptions(builder.Configuration);

// Agregar MemoryCache con límites configurados
var memoryCacheSettings = builder.Configuration.GetSection("MemoryCache").Get<AppSettings.MemoryCacheSettings>() ?? new AppSettings.MemoryCacheSettings();
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = memoryCacheSettings.SizeLimit;
    options.CompactionPercentage = memoryCacheSettings.CompactionPercentage;
    options.ExpirationScanFrequency = TimeSpan.FromSeconds(memoryCacheSettings.ExpirationScanFrequencySeconds);
});

// Registrar el servicio de Circuit Breaker para permisos
builder.Services.AddSingleton<IPermissionCircuitBreakerService, PermissionCircuitBreakerService>();

// Registrar FluentValidation ANTES de MediatR para que los validadores estén disponibles
builder.Services.AddFluentValidation();

builder.Services.AddPermissionModule();
builder.Services.AddPermissionMiddleware();
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddRateLimitingServices(builder.Configuration);
builder.Services.AddRecaptchaServices(builder.Configuration);
RepositoryExtensions.AddRepositories(builder.Services);

// Agregar Prometheus
builder.Services.AddPrometheusMetrics(builder.Configuration);

// Configurar CORS con opciones tipadas
var corsSettings = builder.Configuration.GetSection("Cors").Get<AppSettings.CorsSettings>() ?? new AppSettings.CorsSettings();
builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", policy =>
{
    // Configurar orígenes
    if (corsSettings.AllowedOrigins.Length == 0 || corsSettings.AllowedOrigins.Contains("*"))
    {
        policy.AllowAnyOrigin();
    }
    else
    {
        policy.WithOrigins(corsSettings.AllowedOrigins);

        // AllowCredentials solo puede usarse con orígenes específicos
        if (corsSettings.AllowCredentials)
        {
            policy.AllowCredentials();
        }
    }

    // Configurar métodos
    if (corsSettings.AllowedMethods.Length == 0 || corsSettings.AllowedMethods.Contains("*"))
    {
        policy.AllowAnyMethod();
    }
    else
    {
        policy.WithMethods(corsSettings.AllowedMethods);
    }

    // Configurar headers
    if (corsSettings.AllowedHeaders.Length == 0 || corsSettings.AllowedHeaders.Contains("*"))
    {
        policy.AllowAnyHeader();
    }
    else
    {
        policy.WithHeaders(corsSettings.AllowedHeaders);
    }

    // Configurar headers expuestos
    if (corsSettings.ExposedHeaders.Length > 0)
    {
        policy.WithExposedHeaders(corsSettings.ExposedHeaders);
    }

    // Configurar tiempo de caché para preflight
    policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsSettings.PreflightMaxAgeSeconds));
}));
builder.AddMappings();

// Registrar MediatR DESPUÉS de FluentValidation para que los validadores estén disponibles
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();

    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

WebApplication app = builder.Build();


app.MapOpenApi();
app.MapScalarApiReference("/scalar/v1");

app.UseCors("CorsPolicy");

// Correlation ID debe ir temprano en el pipeline para que todos los logs lo incluyan
app.UseCorrelationId();

// Configurar autenticación y autorización ANTES de mapear endpoints
app.UseServiceDefaults();

// Configurar Prometheus (debe ir temprano en el pipeline)
app.UsePrometheusMetrics(app.Environment);

// Rate Limiting (debe ir antes de los middlewares de autorización)
app.UseRateLimiting();

// Middleware de autorización por permisos
app.UsePermissionAuthorization();

app.MapAuthEndpoints().WithTags("Auth");
app.MapUsersEndpoints().WithTags("Users");
app.MapPermissionsEndpoints().WithTags("Permissions");
app.MapRolesEndpoints().WithTags("Roles");
app.MapPermissionRolesEndpoints().WithTags("PermissionRoles");
app.MapDashboardEndpoints().WithTags("Dashboard");
app.MapPrometheusMetrics();

using (IServiceScope scope = app.Services.CreateScope())
{
    ApiContext context = scope.ServiceProvider.GetRequiredService<ApiContext>();
    //RoleManager<Role> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    await SeedDb.SeedData(context);
}

await app.RunAsync();
