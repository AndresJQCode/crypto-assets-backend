using Api.Application.Dtos.ConnectorDefinition;
using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Domain.Exceptions;
using MediatR;

namespace Api.Application.Commands.ConnectorDefinitionCommands;

internal sealed class SetConnectorDefinitionActiveCommandHandler(
    IConnectorDefinitionRepository repository) : IRequestHandler<SetConnectorDefinitionActiveCommand, ConnectorDefinitionDto>
{
    public async Task<ConnectorDefinitionDto> Handle(SetConnectorDefinitionActiveCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(request.Id, tracking: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Definición de conector no encontrada.");

        if (request.IsActive)
            entity.Activate();
        else
            entity.Deactivate();

        _ = repository.Update(entity);
        _ = await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new ConnectorDefinitionDto
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            LogoUrl = entity.LogoUrl,
            ProviderType = entity.ProviderType,
            CategoryType = entity.CategoryType,
            IsActive = entity.IsActive,
            RequiresOAuth = entity.RequiresOAuth,
            Description = entity.Description
        };
    }
}
