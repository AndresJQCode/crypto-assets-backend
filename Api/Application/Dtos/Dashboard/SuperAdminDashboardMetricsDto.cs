namespace Api.Application.Dtos.Dashboard;

/// <summary>
/// Métricas de tenants (clientes) para el dashboard de SuperAdmin.
/// Incluye totales y comparaciones con período anterior (semana/mes).
/// </summary>
internal sealed class SuperAdminDashboardMetricsDto
{
    /// <summary>Cantidad total de tenants (clientes) en la plataforma.</summary>
    public int TotalTenants { get; set; }

    /// <summary>Tenants registrados hoy (UTC).</summary>
    public int TenantsRegisteredToday { get; set; }

    /// <summary>Tenants registrados en la semana actual (lunes a domingo, UTC).</summary>
    public int TenantsRegisteredThisWeek { get; set; }

    /// <summary>Tenants registrados en la semana anterior (para comparación).</summary>
    public int TenantsRegisteredPreviousWeek { get; set; }

    /// <summary>Variación semanal: diferencia (esta semana - semana anterior).</summary>
    public int WeekOverWeekDifference { get; set; }

    /// <summary>Porcentaje de variación semanal (respecto a la semana anterior). Null si semana anterior es 0.</summary>
    public double? WeekOverWeekChangePercent { get; set; }

    /// <summary>Tenants registrados en el mes actual (UTC).</summary>
    public int TenantsRegisteredThisMonth { get; set; }

    /// <summary>Tenants registrados en el mes anterior (para comparación).</summary>
    public int TenantsRegisteredPreviousMonth { get; set; }

    /// <summary>Variación mensual: diferencia (mes actual - mes anterior).</summary>
    public int MonthOverMonthDifference { get; set; }

    /// <summary>Porcentaje de variación mensual (respecto al mes anterior). Null si mes anterior es 0.</summary>
    public double? MonthOverMonthChangePercent { get; set; }
}
