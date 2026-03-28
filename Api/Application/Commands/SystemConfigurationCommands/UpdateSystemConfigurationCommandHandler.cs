using Domain.AggregatesModel.SystemConfigurationAggregate;
using Domain.Exceptions;
using Infrastructure;
using MediatR;

namespace Api.Application.Commands.SystemConfigurationCommands;

public class UpdateSystemConfigurationCommandHandler(
    ISystemConfigurationRepository repository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<UpdateSystemConfigurationCommand, bool>
{
    public async Task<bool> Handle(
        UpdateSystemConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        var config = await repository.GetByKeyAsync(request.Key, cancellationToken);

        if (config == null)
        {
            throw new DomainException($"System configuration with key '{request.Key}' not found");
        }

        var currentUser = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";

        config.UpdateValue(request.Value, currentUser);

        if (request.Description != null)
        {
            config.UpdateDescription(request.Description, currentUser);
        }

        repository.Update(config);
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
