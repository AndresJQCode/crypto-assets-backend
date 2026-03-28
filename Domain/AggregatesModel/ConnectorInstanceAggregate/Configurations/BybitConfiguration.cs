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

    /// <summary>
    /// Validates that API credentials are present
    /// </summary>
    public override bool IsComplete()
    {
        return !string.IsNullOrWhiteSpace(ApiKey) 
            && !string.IsNullOrWhiteSpace(ApiSecret);
    }

    /// <summary>
    /// Returns masked configuration for display purposes
    /// </summary>
    public override Dictionary<string, string> ToDisplayDictionary()
    {
        return new Dictionary<string, string>
        {
            { "API Key", MaskApiKey(ApiKey) },
            { "Environment", IsTestnet ? "Testnet" : "Mainnet" }
        };
    }

    /// <summary>
    /// Returns equality components for value object comparison
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ApiKey;
        yield return ApiSecret;
        yield return IsTestnet;
    }

    /// <summary>
    /// Masks API key for safe display (shows first 4 and last 4 characters)
    /// </summary>
    private static string MaskApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 8)
            return "****";

        return $"{apiKey[..4]}...{apiKey[^4..]}";
    }
}
