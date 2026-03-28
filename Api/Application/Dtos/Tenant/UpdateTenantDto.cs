namespace Api.Application.Dtos.Tenant;

internal sealed class UpdateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
