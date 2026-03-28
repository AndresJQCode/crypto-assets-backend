using Domain.SeedWork;

namespace Domain.AggregatesModel.ConnectorInstanceAggregate.Configurations;

/// <summary>
/// Base class for connector-specific configurations.
/// Implementations must provide validation logic and equality comparison.
/// </summary>
public abstract class ConnectorConfiguration : ValueObject
{
    /// <summary>
    /// Validates that all required fields for this connector type are present.
    /// </summary>
    /// <returns>True if configuration is complete and valid.</returns>
    public abstract bool IsComplete();

    /// <summary>
    /// Returns a display-friendly dictionary of configuration values for UI rendering.
    /// Sensitive values (API keys, secrets) should be masked or excluded.
    /// </summary>
    /// <returns>Dictionary of key-value pairs for display purposes.</returns>
    public abstract Dictionary<string, string> ToDisplayDictionary();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        // Derived classes must implement GetEqualityComponents
        // This base implementation returns empty to satisfy ValueObject contract
        yield break;
    }
}
