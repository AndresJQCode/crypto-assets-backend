using Domain.AggregatesModel.ConnectorInstanceAggregate.Configurations;
using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.AggregatesModel.ConnectorInstanceAggregate;

/// <summary>
/// Connector aggregate root.
/// Represents an integration with an external platform (e.g., Shopify).
/// </summary>
public class ConnectorInstance : Entity<Guid>, IAggregateRoot
{
    public Guid ConnectorDefinitionId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public ConnectorProviderType ProviderType { get; private set; }
    public ConnectorCategoryType CategoryType { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsEnabled { get; private set; }
    public bool IsConfigured { get; private set; }

    /// <summary>
    /// Serialized configuration as JSON (provider-specific).
    /// Sensitive fields should be encrypted before storage.
    /// </summary>
    public string ConfigurationJson { get; private set; } = default!;

    /// <summary>
    /// OAuth access token (encrypted).
    /// For Shopify, this token does NOT expire unless revoked.
    /// </summary>
    public string? AccessToken { get; private set; }

    /// <summary>
    /// Last successful synchronization timestamp.
    /// </summary>
    public DateTime? LastSyncedAt { get; private set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; private set; }

    // EF Core constructor
    private ConnectorInstance() { }

    /// <summary>
    /// Factory method for creating OAuth connectors (e.g., Shopify).
    /// </summary>
    /// <param name="connectorDefinitionId">Connector definition ID.</param>
    /// <param name="tenantId">Tenant ID.</param>
    /// <param name="userId">User ID (connector owner).</param>
    /// <param name="providerType">Provider type (e.g., Shopify).</param>
    /// <param name="name">Display name for the connector.</param>
    /// <param name="configurationJson">Serialized configuration JSON.</param>
    /// <param name="accessToken">Encrypted OAuth access token.</param>
    /// <returns>New connector instance.</returns>
    public static ConnectorInstance CreateOAuthConnector(
        Guid connectorDefinitionId,
        Guid tenantId,
        Guid userId,
        ConnectorProviderType providerType,
        string name,
        string configurationJson,
        string accessToken)
    {
        ValidateOAuthConnectorType(providerType);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Connector name is required");

        if (string.IsNullOrWhiteSpace(configurationJson))
            throw new DomainException("Configuration is required");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new DomainException("Access token is required for OAuth connectors");

        var connector = new ConnectorInstance
        {
            Id = Guid.CreateVersion7(),
            ConnectorDefinitionId = connectorDefinitionId,
            TenantId = tenantId,
            UserId = userId,
            ProviderType = providerType,
            CategoryType = GetCategoryFromProvider(providerType),
            Name = name.Trim(),
            IsEnabled = true, // OAuth connectors start enabled after successful authentication
            IsConfigured = true, // OAuth flow ensures configuration is complete
            ConfigurationJson = configurationJson,
            AccessToken = accessToken
        };

        // Domain event (infrastructure for future use)
        // connector.AddDomainEvent(new ConnectorCreatedEvent(connector.Id, connector.ProviderType));

        return connector;
    }

    /// <summary>
    /// Updates the connector's display name.
    /// </summary>
    /// <param name="name">New display name.</param>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Connector name is required");

        Name = name.Trim();
    }

    /// <summary>
    /// Updates the connector configuration.
    /// Note: For OAuth connectors, changing scopes requires re-authentication.
    /// </summary>
    /// <param name="configurationJson">New serialized configuration JSON.</param>
    public void UpdateConfiguration(string configurationJson)
    {
        if (!IsEnabled)
            throw new DomainException("Cannot update configuration while connector is disabled");

        if (string.IsNullOrWhiteSpace(configurationJson))
            throw new DomainException("Configuration is required");

        ConfigurationJson = configurationJson;

        // Domain event
        // this.AddDomainEvent(new ConnectorConfigurationUpdatedEvent(this.Id));
    }

    /// <summary>
    /// Enables the connector for active use.
    /// </summary>
    public void Enable()
    {
        if (!IsConfigured)
            throw new DomainException("Cannot enable connector with incomplete configuration");

        if (RequiresOAuth() && string.IsNullOrWhiteSpace(AccessToken))
            throw new DomainException("Cannot enable OAuth connector without access token");

        IsEnabled = true;

        // Domain event
        // this.AddDomainEvent(new ConnectorEnabledEvent(this.Id));
    }

    /// <summary>
    /// Disables the connector without deleting its configuration.
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;

        // Domain event
        // this.AddDomainEvent(new ConnectorDisabledEvent(this.Id));
    }

    /// <summary>
    /// Soft deletes the connector instance.
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        IsEnabled = false;
    }

    /// <summary>
    /// Updates the OAuth access token (e.g., after re-authentication).
    /// </summary>
    /// <param name="accessToken">New encrypted access token.</param>
    public void UpdateAccessToken(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new DomainException("Access token is required");

        if (!RequiresOAuth())
            throw new DomainException("Cannot update access token for non-OAuth connector");

        AccessToken = accessToken;

        // Domain event
        // this.AddDomainEvent(new ConnectorTokenUpdatedEvent(this.Id));
    }

    /// <summary>
    /// Updates the last synchronization timestamp.
    /// </summary>
    public void UpdateLastSync()
    {
        LastSyncedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if this connector type requires OAuth authentication.
    /// API key-based connectors (e.g., Bybit) do NOT require OAuth.
    /// </summary>
    /// <returns>True if OAuth is required.</returns>
    public bool RequiresOAuth()
    {
        return CategoryType == ConnectorCategoryType.Ecommerce;
        // Future: || CategoryType == ConnectorCategoryType.Messaging
        // CryptoExchange and StockBroker use API keys, not OAuth
    }

    /// <summary>
    /// Gets the current status of the connector.
    /// </summary>
    /// <returns>Connector status enum.</returns>
    public ConnectorStatus GetStatus()
    {
        if (!IsConfigured)
            return ConnectorStatus.NotConfigured;

        if (!IsEnabled)
            return ConnectorStatus.Disabled;

        if (RequiresOAuth() && string.IsNullOrWhiteSpace(AccessToken))
            return ConnectorStatus.Error;

        return ConnectorStatus.Active;
    }

    // Private validation helpers

    private static void ValidateOAuthConnectorType(ConnectorProviderType type)
    {
        if (type == ConnectorProviderType.None)
            throw new DomainException("Invalid provider type: None");

        // Currently all provider types require OAuth
        // Future: Add validation for internal connectors (e.g., Coordinadora)
    }

    private static ConnectorCategoryType GetCategoryFromProvider(ConnectorProviderType type)
    {
        return type switch
        {
            ConnectorProviderType.Shopify => ConnectorCategoryType.Ecommerce,
            ConnectorProviderType.Bybit => ConnectorCategoryType.CryptoExchange,
            // Future providers:
            // ConnectorProviderType.EToro => ConnectorCategoryType.StockBroker,
            // ConnectorProviderType.WooCommerce => ConnectorCategoryType.Ecommerce,
            // ConnectorProviderType.WhatsApp => ConnectorCategoryType.Messaging,
            // ConnectorProviderType.Coordinadora => ConnectorCategoryType.Carrier,
            _ => throw new DomainException($"Unknown provider type: {type}")
        };
    }

    /// <summary>
    /// Factory method for creating API key-based connectors (e.g., Bybit, eToro).
    /// These connectors use API key + secret instead of OAuth flow.
    /// </summary>
    /// <param name="connectorDefinitionId">Connector definition ID.</param>
    /// <param name="tenantId">Tenant ID.</param>
    /// <param name="userId">User ID (connector owner).</param>
    /// <param name="providerType">Provider type (e.g., Bybit).</param>
    /// <param name="name">Display name for the connector.</param>
    /// <param name="configurationJson">Serialized configuration JSON (includes encrypted API secret).</param>
    /// <param name="encryptedApiKey">Encrypted API key (stored in AccessToken field).</param>
    /// <returns>New connector instance.</returns>
    public static ConnectorInstance CreateApiKeyConnector(
        Guid connectorDefinitionId,
        Guid tenantId,
        Guid userId,
        ConnectorProviderType providerType,
        string name,
        string configurationJson,
        string encryptedApiKey)
    {
        ValidateApiKeyConnectorType(providerType);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Connector name is required");

        if (string.IsNullOrWhiteSpace(configurationJson))
            throw new DomainException("Configuration is required");

        if (string.IsNullOrWhiteSpace(encryptedApiKey))
            throw new DomainException("API key is required for API key-based connectors");

        var connector = new ConnectorInstance
        {
            Id = Guid.CreateVersion7(),
            ConnectorDefinitionId = connectorDefinitionId,
            TenantId = tenantId,
            UserId = userId,
            ProviderType = providerType,
            CategoryType = GetCategoryFromProvider(providerType),
            Name = name.Trim(),
            IsEnabled = true, // API key connectors start enabled after successful validation
            IsConfigured = true, // Configuration is complete after validation
            ConfigurationJson = configurationJson,
            AccessToken = encryptedApiKey // Reuse AccessToken field for encrypted API key
        };

        // Domain event (infrastructure for future use)
        // connector.AddDomainEvent(new ConnectorCreatedEvent(connector.Id, connector.ProviderType));

        return connector;
    }

    private static void ValidateApiKeyConnectorType(ConnectorProviderType type)
    {
        if (type == ConnectorProviderType.None)
            throw new DomainException("Invalid provider type: None");

        var category = GetCategoryFromProvider(type);
        if (category != ConnectorCategoryType.CryptoExchange && category != ConnectorCategoryType.StockBroker)
            throw new DomainException($"CreateApiKeyConnector should only be used for trading platforms. Use CreateOAuthConnector for {category}");
    }
}
