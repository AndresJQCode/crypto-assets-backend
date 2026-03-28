using Api.Application.Dtos.Dashboard;
using Domain.AggregatesModel.TenantAggregate;
using MediatR;

namespace Api.Application.Queries.DashboardQueries;

internal sealed class GetSuperAdminDashboardMetricsQueryHandler(ITenantRepository tenantRepository)
    : IRequestHandler<GetSuperAdminDashboardMetricsQuery, SuperAdminDashboardMetricsDto>
{
    public async Task<SuperAdminDashboardMetricsDto> Handle(
        GetSuperAdminDashboardMetricsQuery request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var todayStart = new DateTimeOffset(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, TimeSpan.Zero);
        var todayEnd = todayStart.AddDays(1);

        // Semana actual: lunes a domingo (ISO 8601)
        int daysFromMonday = ((int)utcNow.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var thisWeekStart = todayStart.AddDays(-daysFromMonday);
        var thisWeekEnd = thisWeekStart.AddDays(7);
        var previousWeekStart = thisWeekStart.AddDays(-7);
        var previousWeekEnd = thisWeekStart;

        // Mes actual y mes anterior
        var thisMonthStart = new DateTimeOffset(utcNow.Year, utcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var thisMonthEnd = thisMonthStart.AddMonths(1);
        var previousMonthStart = thisMonthStart.AddMonths(-1);
        var previousMonthEnd = thisMonthStart;

        int totalTenants = await tenantRepository.GetCountByFilter(_ => true, cancellationToken);
        int tenantsToday = await tenantRepository.GetCountByFilter(
            t => t.CreatedOn >= todayStart && t.CreatedOn < todayEnd, cancellationToken);
        int tenantsThisWeek = await tenantRepository.GetCountByFilter(
            t => t.CreatedOn >= thisWeekStart && t.CreatedOn < thisWeekEnd, cancellationToken);
        int tenantsPreviousWeek = await tenantRepository.GetCountByFilter(
            t => t.CreatedOn >= previousWeekStart && t.CreatedOn < previousWeekEnd, cancellationToken);
        int tenantsThisMonth = await tenantRepository.GetCountByFilter(
            t => t.CreatedOn >= thisMonthStart && t.CreatedOn < thisMonthEnd, cancellationToken);
        int tenantsPreviousMonth = await tenantRepository.GetCountByFilter(
            t => t.CreatedOn >= previousMonthStart && t.CreatedOn < previousMonthEnd, cancellationToken);

        int weekDiff = tenantsThisWeek - tenantsPreviousWeek;
        double? weekChangePercent = tenantsPreviousWeek > 0
            ? Math.Round((double)(tenantsThisWeek - tenantsPreviousWeek) / tenantsPreviousWeek * 100, 2)
            : null;

        int monthDiff = tenantsThisMonth - tenantsPreviousMonth;
        double? monthChangePercent = tenantsPreviousMonth > 0
            ? Math.Round((double)(tenantsThisMonth - tenantsPreviousMonth) / tenantsPreviousMonth * 100, 2)
            : null;

        return new SuperAdminDashboardMetricsDto
        {
            TotalTenants = totalTenants,
            TenantsRegisteredToday = tenantsToday,
            TenantsRegisteredThisWeek = tenantsThisWeek,
            TenantsRegisteredPreviousWeek = tenantsPreviousWeek,
            WeekOverWeekDifference = weekDiff,
            WeekOverWeekChangePercent = weekChangePercent,
            TenantsRegisteredThisMonth = tenantsThisMonth,
            TenantsRegisteredPreviousMonth = tenantsPreviousMonth,
            MonthOverMonthDifference = monthDiff,
            MonthOverMonthChangePercent = monthChangePercent
        };
    }
}
