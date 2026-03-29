using Api.Application.Dtos.Dashboard;
using Api.Application.Queries.DashboardQueries;
using MediatR;

namespace Api.Apis.DashboardEndpoints;

internal static class DashboardApi
{
    public static RouteGroupBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder api = app.MapGroup("dashboard");

        // GET /dashboard/metrics - Obtiene las estadísticas generales del dashboard
        api.MapGet("/metrics", async (IMediator mediator) =>
        {
            var metrics = await mediator.Send(new GetDashboardMetricsQuery());
            return metrics;
        })
        .WithName("GetDashboardMetrics")
        .WithSummary("Obtener métricas generales del dashboard")
        .WithDescription("Obtiene las métricas generales del dashboard. Requiere permiso: Dashboard.Read")
        .RequireAuthorization()
        .Produces<IEnumerable<DashboardMetricsDto>>();

        return api;
    }


}
