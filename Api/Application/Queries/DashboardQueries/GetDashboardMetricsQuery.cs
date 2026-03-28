using Api.Application.Dtos.Dashboard;
using MediatR;

namespace Api.Application.Queries.DashboardQueries;

internal sealed class GetDashboardMetricsQuery : IRequest<IEnumerable<DashboardMetricsDto>>
{
}
