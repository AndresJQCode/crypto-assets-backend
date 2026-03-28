using Api.Application.Dtos.Dashboard;
using Api.Application.Queries.DashboardQueries;
using MediatR;

namespace Api.Apis.Admin.DashboardEndpoints;

internal static class AdminDashboardApi
{
    public static RouteGroupBuilder MapAdminDashboardEndpoints(this RouteGroupBuilder adminGroup)
    {
        RouteGroupBuilder group = adminGroup.MapGroup("/dashboard")
            .WithTags("Admin - Dashboard");

        group.MapGet("/metrics", async (IMediator mediator) =>
        {
            var metrics = await mediator.Send(new GetSuperAdminDashboardMetricsQuery());
            return Results.Ok(metrics);
        })
        .WithName("GetSuperAdminDashboardMetrics")
        .WithSummary("Métricas de tenants para SuperAdmin")
        .WithDescription("Obtiene cantidad total de tenants (clientes), registrados hoy, esta semana, este mes y comparación con período anterior.")
        .Produces<SuperAdminDashboardMetricsDto>();

        return group;
    }
}
