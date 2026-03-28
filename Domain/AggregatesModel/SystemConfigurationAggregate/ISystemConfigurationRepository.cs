using Domain.SeedWork;

namespace Domain.AggregatesModel.SystemConfigurationAggregate;

public interface ISystemConfigurationRepository : IRepository<SystemConfiguration>
{
    Task<SystemConfiguration?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> IsFeatureEnabledAsync(string key, CancellationToken cancellationToken = default);
}
