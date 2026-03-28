using Api.Application.Dtos.Tenant;
using MediatR;

namespace Api.Application.Commands.TenantCommands;

internal sealed class UpdateTenantCommand : IRequest<TenantDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
