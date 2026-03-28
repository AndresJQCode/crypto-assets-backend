using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Domain.Exceptions;
using MediatR;

namespace Api.Application.Commands.ConnectorDefinitionCommands;

internal sealed class DeleteConnectorDefinitionCommandHandler(
    IConnectorDefinitionRepository repository) : IRequestHandler<DeleteConnectorDefinitionCommand, Unit>
{
    public async Task<Unit> Handle(DeleteConnectorDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(request.Id, tracking: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Definición de conector no encontrada.");

        entity.Delete();
        _ = repository.Update(entity);
        _ = await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Unit.Value;
    }
}
