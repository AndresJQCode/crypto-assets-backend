using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.AggregatesModel.ConnectorInstanceAggregate.Configurations;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Services.Connectors;
using Infrastructure.Services.Security;

namespace Api.Extensions;

internal static class ConnectorExtensions
{
    public static IServiceCollection AddConnectorServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Get connector configuration for timeouts
        var connectorsSettings = configuration.GetSection("Connectors").Get<AppSettings.ConnectorsSettings>();
        var shopifyTimeout = connectorsSettings?.Shopify?.TimeoutSeconds ?? 30;

        // Register OAuth services with configured timeouts
        services.AddHttpClient<IConnectorOAuthService<ShopifyConfiguration>, ShopifyOAuthService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(shopifyTimeout);
        });

        // Register encryption service
        services.AddSingleton<IEncryptionService, AesEncryptionService>();

        // Register connector repositories
        services.AddScoped<IConnectorInstanceRepository, ConnectorInstanceRepository>();
        services.AddScoped<IConnectorDefinitionRepository, ConnectorDefinitionRepository>();

        return services;
    }
}
