using Api.Application.Dtos.ConnectorDefinition;
using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Domain.Exceptions;
using MediatR;

namespace Api.Application.Commands.ConnectorDefinitionCommands;

internal sealed class CreateConnectorDefinitionCommandHandler(
    IConnectorDefinitionRepository repository) : IRequestHandler<CreateConnectorDefinitionCommand, ConnectorDefinitionDto>
{
    public async Task<ConnectorDefinitionDto> Handle(CreateConnectorDefinitionCommand request, CancellationToken cancellationToken)
    {
        if (await repository.ExistsWithNameAsync(request.Name, null, cancellationToken))
            throw new BadRequestException("Ya existe una definición de conector con ese nombre.");

        if (await repository.GetByProviderTypeAsync(request.ProviderType, cancellationToken) != null)
            throw new BadRequestException("Ya existe una definición de conector con ese ProviderType.");

        Uri? logoUrl = null;
        if (!string.IsNullOrWhiteSpace(request.LogoUrl) && Uri.TryCreate(request.LogoUrl, UriKind.Absolute, out var parsed))
            logoUrl = parsed;

        var entity = ConnectorDefinition.Create(
            request.Name,
            request.ProviderType,
            request.CategoryType,
            request.RequiresOAuth,
            logoUrl,
            request.Description);

        await repository.Create(entity, cancellationToken);
        _ = await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return MapToDto(entity);
    }

    private static ConnectorDefinitionDto MapToDto(ConnectorDefinition e) => new()
    {
        Id = e.Id.ToString(),
        Name = e.Name,
        LogoUrl = e.LogoUrl,
        ProviderType = e.ProviderType,
        CategoryType = e.CategoryType,
        IsActive = e.IsActive,
        RequiresOAuth = e.RequiresOAuth,
        Description = e.Description
    };
}
