using Api.Application.Dtos.SystemConfiguration;
using Domain.AggregatesModel.SystemConfigurationAggregate;
using MediatR;

namespace Api.Application.Queries.SystemConfigurationQueries;

public class GetSystemConfigurationByKeyQueryHandler(
    ISystemConfigurationRepository repository)
    : IRequestHandler<GetSystemConfigurationByKeyQuery, SystemConfigurationDto?>
{
    public async Task<SystemConfigurationDto?> Handle(
        GetSystemConfigurationByKeyQuery request,
        CancellationToken cancellationToken)
    {
        var config = await repository.GetByKeyAsync(request.Key, cancellationToken);

        if (config == null)
            return null;

        return new SystemConfigurationDto(
            config.Id,
            config.Key,
            config.Value,
            config.Description,
            config.IsActive,
            config.LastModifiedOn,
            config.LastModifiedByName
        );
    }
}
