using Api.Application.Dtos.ConnectorDefinition;
using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Domain.Exceptions;
using MediatR;

namespace Api.Application.Commands.ConnectorDefinitionCommands;

internal sealed class UpdateConnectorDefinitionCommandHandler(
    IConnectorDefinitionRepository repository) : IRequestHandler<UpdateConnectorDefinitionCommand, ConnectorDefinitionDto>
{
    public async Task<ConnectorDefinitionDto> Handle(UpdateConnectorDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(request.Id, tracking: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Definición de conector no encontrada.");

        if (await repository.ExistsWithNameAsync(request.Name, request.Id, cancellationToken))
            throw new BadRequestException("Ya existe otra definición de conector con ese nombre.");

        Uri? logoUrl = null;
        if (!string.IsNullOrWhiteSpace(request.LogoUrl) && Uri.TryCreate(request.LogoUrl, UriKind.Absolute, out var parsed))
            logoUrl = parsed;

        entity.Update(request.Name, logoUrl, request.Description);
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
