using Domain.AggregatesModel.RoleAggregate;

namespace Api.Extensions;

/// <summary>
/// Extension methods for Super Admin authorization
/// </summary>
public static class AdminExtensions
{
    /// <summary>
    /// Requires the user to be a Super Admin
    /// Applies to individual route handlers
    /// </summary>
    public static RouteHandlerBuilder RequireSuperAdminRole(
        this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            // Check if user is authenticated
            if (user.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            // Check if user has SuperAdmin role
            if (!user.IsInRole(RolesEnum.SuperAdmin.ToString()))
            {
                return Results.Problem(
                    statusCode: 403,
                    title: "Forbidden",
                    detail: "This endpoint requires SuperAdmin role");
            }

            return await next(context);
        });
    }

    /// <summary>
    /// Requires the user to be a Super Admin
    /// Applies to entire route groups
    /// </summary>
    public static RouteGroupBuilder RequireSuperAdminRole(
        this RouteGroupBuilder group)
    {
        return group.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            // Check if user is authenticated
            if (user.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            // Check if user has SuperAdmin role
            if (!user.IsInRole(RolesEnum.SuperAdmin.ToString()))
            {
                return Results.Problem(
                    statusCode: 403,
                    title: "Forbidden",
                    detail: "Admin endpoints require SuperAdmin role");
            }

            return await next(context);
        });
    }

    /// <summary>
    /// Maps admin endpoints with /admin prefix and SuperAdmin requirement
    /// </summary>
    public static RouteGroupBuilder MapAdminGroup(
        this IEndpointRouteBuilder app,
        string prefix = "/admin")
    {
        return app.MapGroup(prefix)
            .RequireAuthorization()
            .RequireSuperAdminRole();
    }
}
