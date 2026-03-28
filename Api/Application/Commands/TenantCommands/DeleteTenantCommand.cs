using MediatR;

namespace Api.Application.Commands.TenantCommands;

internal sealed class DeleteTenantCommand(Guid id) : IRequest<Unit>
{
    public Guid Id { get; } = id;
}
