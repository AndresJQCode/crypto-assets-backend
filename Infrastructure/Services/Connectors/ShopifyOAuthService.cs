using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.AggregatesModel.ConnectorInstanceAggregate.Configurations;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Connectors;

/// <summary>
/// Shopify OAuth 2.0 service implementation.
/// Handles authentication flow and API interactions with Shopify.
/// </summary>
public class ShopifyOAuthService(
    IHttpClientFactory httpClientFactory,
    IOptions<AppSettings.ConnectorsSettings> connectorsSettings,
    ILogger<ShopifyOAuthService> logger)
    : IConnectorOAuthService<ShopifyConfiguration>
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly AppSettings.ConnectorsSettings.ShopifyConnectorSettings _shopifySettings = connectorsSettings.Value.Shopify
        ?? throw new ArgumentNullException(nameof(connectorsSettings), "Shopify configuration is missing");
    private readonly ILogger<ShopifyOAuthService> _logger = logger;

    /// <summary>
    /// Generates the Shopify OAuth authorization URL.
    /// </summary>
    public Task<string> GetAuthorizationUrlAsync(string shopDomain, string state, IEnumerable<string>? scopes = null)
    {
        // Validate shop domain format
        if (string.IsNullOrWhiteSpace(shopDomain))
            throw new DomainException("Shop domain is required");

        // Normalize shop domain (ensure .myshopify.com suffix)
        var normalizedDomain = NormalizeShopDomain(shopDomain);

        // Use provided scopes or default from configuration
        var requestedScopes = scopes ?? _shopifySettings.Scopes;
        var scopeString = string.Join(",", requestedScopes);

        // Build OAuth URL
        var authUrl = $"https://{normalizedDomain}/admin/oauth/authorize" +
            $"?client_id={_shopifySettings.ClientId}" +
            $"&scope={Uri.EscapeDataString(scopeString)}" +
            $"&redirect_uri={Uri.EscapeDataString(_shopifySettings.RedirectUri)}" +
            $"&state={state}";

        _logger.LogInformation(
            "Generated Shopify OAuth URL for shop {ShopDomain} with scopes {Scopes}",
            normalizedDomain,
            scopeString);

        return Task.FromResult(authUrl);
    }

    /// <summary>
    /// Exchanges authorization code for access token.
    /// </summary>
    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string shopDomain, string code)
    {
        if (string.IsNullOrWhiteSpace(shopDomain))
            throw new DomainException("Shop domain is required");

        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Authorization code is required");

        var normalizedDomain = NormalizeShopDomain(shopDomain);

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var tokenUrl = $"https://{normalizedDomain}/admin/oauth/access_token";

            var requestBody = new
            {
                client_id = _shopifySettings.ClientId,
                client_secret = _shopifySettings.ClientSecret,
                code
            };

            var response = await httpClient.PostAsJsonAsync(tokenUrl, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Failed to exchange code for token. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode,
                    errorContent);
                throw new DomainException($"Failed to obtain Shopify access token: {response.StatusCode}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<ShopifyTokenResponse>();

            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new DomainException("Invalid token response from Shopify");
            }

            _logger.LogInformation(
                "Successfully exchanged OAuth code for access token for shop {ShopDomain}",
                normalizedDomain);

            return new OAuthTokenResponse(
                tokenResponse.AccessToken,
                "Bearer",
                tokenResponse.Scope);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while exchanging code for token for shop {ShopDomain}", normalizedDomain);
            throw new DomainException("Failed to communicate with Shopify OAuth service", ex);
        }
    }

    /// <summary>
    /// Validates the connection by making a test API call to Shopify.
    /// </summary>
    public async Task<bool> ValidateConnectionAsync(string shopDomain, string accessToken)
    {
        if (string.IsNullOrWhiteSpace(shopDomain))
            throw new DomainException("Shop domain is required");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new DomainException("Access token is required");

        var normalizedDomain = NormalizeShopDomain(shopDomain);

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", accessToken);

            var shopUrl = new Uri($"https://{normalizedDomain}/admin/api/{_shopifySettings.ApiVersion}/shop.json");
            var response = await httpClient.GetAsync(shopUrl);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Connection validation successful for shop {ShopDomain}", normalizedDomain);
                return true;
            }

            _logger.LogWarning(
                "Connection validation failed for shop {ShopDomain}. Status: {StatusCode}",
                normalizedDomain,
                response.StatusCode);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while validating connection for shop {ShopDomain}", normalizedDomain);
            return false;
        }
    }

    /// <summary>
    /// Fetches shop metadata from Shopify API.
    /// </summary>
    public async Task<ShopifyConfiguration> FetchConnectionMetadataAsync(string shopDomain, string accessToken)
    {
        if (string.IsNullOrWhiteSpace(shopDomain))
            throw new DomainException("Shop domain is required");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new DomainException("Access token is required");

        var normalizedDomain = NormalizeShopDomain(shopDomain);

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", accessToken);

            var shopUrl = new Uri($"https://{normalizedDomain}/admin/api/{_shopifySettings.ApiVersion}/shop.json");
            var response = await httpClient.GetAsync(shopUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Failed to fetch shop metadata. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode,
                    errorContent);
                throw new DomainException($"Failed to fetch Shopify shop metadata: {response.StatusCode}");
            }

            var shopResponse = await response.Content.ReadFromJsonAsync<ShopifyShopResponse>();

            if (shopResponse?.Shop == null)
            {
                throw new DomainException("Invalid shop response from Shopify");
            }

            _logger.LogInformation(
                "Successfully fetched metadata for shop {ShopDomain} (Name: {ShopName})",
                normalizedDomain,
                shopResponse.Shop.Name);

            // Extract scopes from the granted permissions (if available in response headers)
            var grantedScopes = response.Headers.TryGetValues("X-Shopify-API-Request-Limit", out var scopeValues)
                ? _shopifySettings.Scopes // Fallback to configured scopes
                : _shopifySettings.Scopes;

            return ShopifyConfiguration.Create(
                normalizedDomain,
                _shopifySettings.ApiVersion,
                grantedScopes);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching shop metadata for shop {ShopDomain}", normalizedDomain);
            throw new DomainException("Failed to communicate with Shopify API", ex);
        }
    }

    // Private helper methods

    /// <summary>
    /// Normalizes shop domain to ensure .myshopify.com suffix.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Shopify domains must be lowercase")]
    private static string NormalizeShopDomain(string shopDomain)
    {
        var domain = shopDomain.Trim().ToLowerInvariant();

        // Remove protocol if present
        domain = domain.Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                       .Replace("http://", "", StringComparison.OrdinalIgnoreCase);

        // Remove trailing slash
        domain = domain.TrimEnd('/');

        // Ensure .myshopify.com suffix (unless already present)
        if (!domain.EndsWith(".myshopify.com", StringComparison.OrdinalIgnoreCase))
        {
            // Extract shop name if full domain provided
            var shopName = domain.Split('.')[0];
            domain = $"{shopName}.myshopify.com";
        }

        return domain;
    }

    // DTOs for Shopify API responses

    private sealed record ShopifyTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("scope")] string Scope);

    private sealed record ShopifyShopResponse(
        [property: JsonPropertyName("shop")] ShopifyShop Shop);

    private sealed record ShopifyShop(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("domain")] string Domain,
        [property: JsonPropertyName("myshopify_domain")] string MyshopifyDomain);
}
