using Domain.SeedWork;

namespace Domain.AggregatesModel.ConnectorInstanceAggregate;

/// <summary>
/// Repository interface for ConnectorInstance aggregate.
/// Extends generic repository with connector-specific queries.
/// </summary>
public interface IConnectorInstanceRepository : IRepository<ConnectorInstance>
{
    /// <summary>
    /// Gets a connector by ID with decrypted access token.
    /// </summary>
    /// <param name="id">ConnectorInstance ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>ConnectorInstance with decrypted token, or null if not found.</returns>
    Task<ConnectorInstance?> GetByIdWithDecryptedTokenAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all connector instances for a specific tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of connector instances for the tenant.</returns>
    Task<List<ConnectorInstance>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a connector already exists for a user and provider type.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="providerType">Provider type (e.g., Shopify).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if connector exists, false otherwise.</returns>
    Task<bool> ExistsForUserAsync(Guid userId, ConnectorProviderType providerType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a connector instance by user ID and provider type.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="providerType">Provider type (e.g., Bybit).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>ConnectorInstance if found, null otherwise.</returns>
    Task<ConnectorInstance?> GetByUserAndProviderAsync(Guid userId, ConnectorProviderType providerType, CancellationToken cancellationToken = default);
}
