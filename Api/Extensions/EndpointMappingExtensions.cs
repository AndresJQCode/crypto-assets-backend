namespace Api.Extensions;

/// <summary>
/// Extension methods for mapping endpoint groups
/// </summary>
public static class EndpointMappingExtensions
{
    /// <summary>
    /// Maps tenant endpoints with /api prefix
    /// All endpoints require authentication and tenant context
    /// </summary>
    public static RouteGroupBuilder MapTenantGroup(
        this IEndpointRouteBuilder app,
        string prefix = "/")
    {
        return app.MapGroup(prefix)
            .RequireAuthorization();
    }

    /// <summary>
    /// Maps public endpoints (no authentication required)
    /// </summary>
    public static RouteGroupBuilder MapPublicGroup(
        this IEndpointRouteBuilder app,
        string prefix = "/")
    {
        return app.MapGroup(prefix);
    }
}
