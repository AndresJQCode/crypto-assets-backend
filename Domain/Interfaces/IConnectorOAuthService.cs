namespace Domain.Interfaces;

/// <summary>
/// Generic interface for OAuth-based connector services.
/// Provides authentication and connection management for external platforms.
/// </summary>
/// <typeparam name="TConfiguration">The connector-specific configuration type.</typeparam>
public interface IConnectorOAuthService<TConfiguration> where TConfiguration : class
{
    /// <summary>
    /// Generates the OAuth authorization URL for initiating the OAuth flow.
    /// </summary>
    /// <param name="shopDomain">The shop/domain identifier for the connector (e.g., "mystore.myshopify.com").</param>
    /// <param name="state">CSRF protection state token (JWT containing TenantId, UserId).</param>
    /// <param name="scopes">Optional list of OAuth scopes to request.</param>
    /// <returns>The authorization URL to redirect the user to.</returns>
    Task<string> GetAuthorizationUrlAsync(string shopDomain, string state, IEnumerable<string>? scopes = null);

    /// <summary>
    /// Exchanges the OAuth authorization code for an access token.
    /// </summary>
    /// <param name="shopDomain">The shop/domain identifier.</param>
    /// <param name="code">The authorization code received from the OAuth callback.</param>
    /// <returns>OAuth token response containing access token and related data.</returns>
    Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string shopDomain, string code);

    /// <summary>
    /// Validates that the connector is still active by making a test API call.
    /// </summary>
    /// <param name="shopDomain">The shop/domain identifier.</param>
    /// <param name="accessToken">The access token to validate.</param>
    /// <returns>True if the connection is valid, false otherwise.</returns>
    Task<bool> ValidateConnectionAsync(string shopDomain, string accessToken);

    /// <summary>
    /// Fetches metadata about the connected shop/account.
    /// Used after OAuth completion to populate connector configuration.
    /// </summary>
    /// <param name="shopDomain">The shop/domain identifier.</param>
    /// <param name="accessToken">The access token to use for the API call.</param>
    /// <returns>Connector-specific metadata (shop name, plan, etc.).</returns>
    Task<TConfiguration> FetchConnectionMetadataAsync(string shopDomain, string accessToken);
}

/// <summary>
/// OAuth token response from the connector provider.
/// </summary>
public record OAuthTokenResponse(
    string AccessToken,
    string? TokenType = "Bearer",
    string? Scope = null
);
