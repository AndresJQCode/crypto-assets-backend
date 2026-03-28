using System.Diagnostics.CodeAnalysis;

namespace Domain.AggregatesModel.ConnectorInstanceAggregate.Configurations;

/// <summary>
/// Shopify-specific connector configuration.
/// Contains shop domain, API version, and OAuth scopes.
/// </summary>
public class ShopifyConfiguration : ConnectorConfiguration
{
    /// <summary>
    /// Shopify shop domain (e.g., "mystore.myshopify.com")
    /// </summary>
    public string ShopDomain { get; private set; } = default!;

    /// <summary>
    /// Shopify API version (e.g., "2024-01")
    /// </summary>
    public string ApiVersion { get; private set; } = default!;

    /// <summary>
    /// OAuth scopes granted to the app (e.g., ["read_orders", "write_products"])
    /// </summary>
    public IReadOnlyCollection<string> Scopes { get; private set; } = default!;

    private ShopifyConfiguration() { }

    /// <summary>
    /// Creates a new Shopify configuration.
    /// </summary>
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Shopify shop domains and OAuth scopes are case-insensitive and conventionally lowercase")]
    public static ShopifyConfiguration Create(
        string shopDomain,
        string apiVersion,
        IReadOnlyCollection<string> scopes)
    {
        if (string.IsNullOrWhiteSpace(shopDomain))
            throw new ArgumentException("Shop domain is required", nameof(shopDomain));

        if (string.IsNullOrWhiteSpace(apiVersion))
            throw new ArgumentException("API version is required", nameof(apiVersion));

        if (scopes == null || scopes.Count == 0)
            throw new ArgumentException("At least one scope is required", nameof(scopes));

        return new ShopifyConfiguration
        {
            ShopDomain = shopDomain.Trim().ToLowerInvariant(),
            ApiVersion = apiVersion.Trim(),
            Scopes = scopes.Select(s => s.Trim().ToLowerInvariant()).ToList()
        };
    }

    public override bool IsComplete()
    {
        return !string.IsNullOrWhiteSpace(ShopDomain) &&
               !string.IsNullOrWhiteSpace(ApiVersion) &&
               Scopes != null &&
               Scopes.Count > 0;
    }

    public override Dictionary<string, string> ToDisplayDictionary()
    {
        return new Dictionary<string, string>
        {
            { "Shop Domain", ShopDomain },
            { "API Version", ApiVersion },
            { "Scopes", string.Join(", ", Scopes) }
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ShopDomain;
        yield return ApiVersion;
        foreach (var scope in Scopes.OrderBy(s => s))
        {
            yield return scope;
        }
    }
}
