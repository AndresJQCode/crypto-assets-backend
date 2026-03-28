using Domain.AggregatesModel.SystemConfigurationAggregate;
using Domain.Exceptions;
using MediatR;

namespace Api.Application.Commands.SystemConfigurationCommands;

public class ToggleSystemConfigurationCommandHandler(
    ISystemConfigurationRepository repository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<ToggleSystemConfigurationCommand, bool>
{
    public async Task<bool> Handle(
        ToggleSystemConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        var config = await repository.GetByKeyAsync(request.Key, cancellationToken);

        if (config == null)
        {
            throw new DomainException($"System configuration with key '{request.Key}' not found");
        }

        var currentUser = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";

        if (request.IsActive)
        {
            config.Activate(currentUser);
        }
        else
        {
            config.Deactivate(currentUser);
        }

        repository.Update(config);
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
