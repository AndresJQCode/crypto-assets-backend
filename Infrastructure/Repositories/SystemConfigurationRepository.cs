using Domain.AggregatesModel.SystemConfigurationAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class SystemConfigurationRepository : Repository<SystemConfiguration>, ISystemConfigurationRepository
{
    private readonly ApiContext _context;

    public SystemConfigurationRepository(
        ApiContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SystemConfigurationRepository> logger)
        : base(context, httpContextAccessor, logger)
    {
        _context = context;
    }

    public async Task<SystemConfiguration?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return await _context.SystemConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(sc => sc.Key == key, cancellationToken);
    }

    public async Task<bool> IsFeatureEnabledAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var config = await GetByKeyAsync(key, cancellationToken);

        if (config == null || !config.IsActive)
            return false;

        return config.GetBoolValue();
    }
}
