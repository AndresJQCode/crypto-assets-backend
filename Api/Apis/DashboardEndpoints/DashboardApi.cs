using Api.Application.Dtos.Dashboard;
using Api.Application.Queries.DashboardQueries;
using MediatR;

namespace Api.Apis.DashboardEndpoints;

internal static class DashboardApi
{
    public static RouteGroupBuilder MapDashboardEndpoints(this RouteGroupBuilder tenantGroup)
    {
        RouteGroupBuilder api = tenantGroup.MapGroup("/dashboard")
            .WithTags("Tenant - Dashboard");

        // GET /api/dashboard/metrics - Obtiene las estadísticas generales del dashboard
        api.MapGet("/metrics", async (IMediator mediator) =>
        {
            var metrics = await mediator.Send(new GetDashboardMetricsQuery());
            return metrics;
        })
        .WithName("GetDashboardMetrics")
        .WithSummary("Obtener métricas generales del dashboard")
        .WithDescription("Obtiene las métricas generales del dashboard para el tenant actual.")
        .Produces<IEnumerable<DashboardMetricsDto>>();

        return api;
    }


}
