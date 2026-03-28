using Api.Application.Dtos.Permission;

namespace Api.Application.Dtos.Auth;

internal sealed class AuthUserDto
{
    public string Id { get; set; } = string.Empty;
    public required string Email { get; set; }
    public required string Name { get; set; }
    /// <summary>Número de WhatsApp del usuario.</summary>
    public string? WhatsAppNumber { get; set; }
    /// <summary>Null = usuario plataforma (SuperAdmin).</summary>
    public string? TenantId { get; set; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public IReadOnlyCollection<UserPermissionDto> Permissions { get; init; } = [];
}
