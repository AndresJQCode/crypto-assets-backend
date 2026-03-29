namespace Application.Dtos.Dashboard;

internal sealed class DashboardMetricsDto
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
