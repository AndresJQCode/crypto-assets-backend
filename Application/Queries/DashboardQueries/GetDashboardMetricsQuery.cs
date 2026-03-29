using Application.Dtos.Dashboard;
using MediatR;

namespace Application.Queries.DashboardQueries;

internal sealed class GetDashboardMetricsQuery : IRequest<IEnumerable<DashboardMetricsDto>>
{
}
