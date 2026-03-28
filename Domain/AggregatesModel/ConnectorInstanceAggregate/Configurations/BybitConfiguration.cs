namespace Domain.AggregatesModel.ConnectorInstanceAggregate.Configurations;

/// <summary>
/// Configuration for Bybit trading connector.
/// Stores encrypted API credentials.
/// </summary>
public class BybitConfiguration : ConnectorConfiguration
{
    /// <summary>
    /// Bybit API Key (encrypted in storage)
    /// </summary>
    public required string ApiKey { get; init; }

    /// <summary>
    /// Bybit API Secret (encrypted in storage)
    /// </summary>
    public required string ApiSecret { get; init; }

    /// <summary>
    /// Whether to use Bybit testnet instead of mainnet
    /// </summary>
    public bool IsTestnet { get; init; }
}
