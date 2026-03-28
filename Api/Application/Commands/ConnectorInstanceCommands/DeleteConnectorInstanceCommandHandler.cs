using Api.Infrastructure.Services;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.Exceptions;
using MediatR;

namespace Api.Application.Commands.ConnectorInstanceCommands;

internal sealed class DeleteConnectorInstanceCommandHandler(
    IConnectorInstanceRepository repository,
    ITenantContext tenantContext) : IRequestHandler<DeleteConnectorInstanceCommand, Unit>
{
    public async Task<Unit> Handle(DeleteConnectorInstanceCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(request.Id, tracking: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Conector no encontrado.");

        if (entity.IsDeleted)
            throw new NotFoundException("Conector no encontrado.");

        var tenantId = tenantContext.GetCurrentTenantId();
        if (tenantId.HasValue && entity.TenantId != tenantId.Value)
            throw new BadRequestException("No tiene permiso para eliminar este conector.");

        entity.Delete();
        _ = repository.Update(entity);
        _ = await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Unit.Value;
    }
}
