using Domain.SeedWork;

namespace Domain.AggregatesModel.ConnectorDefinitionAggregate;

/// <summary>
/// Repository interface for ConnectorDefinition aggregate.
/// </summary>
public interface IConnectorDefinitionRepository : IRepository<ConnectorDefinition>
{
    /// <summary>
    /// Gets all active connector definitions.
    /// </summary>
    Task<List<ConnectorDefinition>> GetActiveDefinitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a connector definition by provider type.
    /// </summary>
    Task<ConnectorDefinition?> GetByProviderTypeAsync(string providerType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a connector definition exists with the given name.
    /// </summary>
    Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
