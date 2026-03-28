using System.Diagnostics.CodeAnalysis;
using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.AggregatesModel.ConnectorDefinitionAggregate;

/// <summary>
/// ConnectorDefinition aggregate root.
/// Represents a connector type available in the system (managed by super admin).
/// </summary>
public class ConnectorDefinition : Entity<Guid>, IAggregateRoot
{
    public string Name { get; private set; } = default!;

    [SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "String is easier to configure and store in database")]
    public string? LogoUrl { get; private set; }
    public string ProviderType { get; private set; } = default!;
    public string CategoryType { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public bool RequiresOAuth { get; private set; }
    public string? Description { get; private set; }
    public bool IsDeleted { get; private set; }

    // EF Core constructor
    private ConnectorDefinition() { }

    /// <summary>
    /// Creates a new connector definition.
    /// </summary>
    public static ConnectorDefinition Create(
        string name,
        string providerType,
        string categoryType,
        bool requiresOAuth,
        Uri? logoUrl = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Connector definition name is required");

        if (string.IsNullOrWhiteSpace(providerType))
            throw new DomainException("Provider type is required");

        if (string.IsNullOrWhiteSpace(categoryType))
            throw new DomainException("Category type is required");

        return new ConnectorDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            ProviderType = providerType.Trim(),
            CategoryType = categoryType.Trim(),
            RequiresOAuth = requiresOAuth,
            LogoUrl = logoUrl?.ToString(),
            Description = description?.Trim(),
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the connector definition details.
    /// </summary>
    public void Update(
        string name,
        Uri? logoUrl = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Connector definition name is required");

        Name = name.Trim();
        LogoUrl = logoUrl?.ToString();
        Description = description?.Trim();
    }

    /// <summary>
    /// Activates the connector definition making it available for tenants.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the connector definition preventing new tenant instances.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Soft deletes the connector definition.
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        IsActive = false;
    }
}
