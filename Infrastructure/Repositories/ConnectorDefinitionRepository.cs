using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class ConnectorDefinitionRepository(
    ApiContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ConnectorDefinitionRepository> logger)
    : Repository<ConnectorDefinition>(context, httpContextAccessor, logger), IConnectorDefinitionRepository
{
    private readonly ApiContext _context = context;

    public async Task<List<ConnectorDefinition>> GetActiveDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ConnectorDefinitions
            .Where(cd => cd.IsActive)
            .OrderBy(cd => cd.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConnectorDefinition?> GetByProviderTypeAsync(string providerType, CancellationToken cancellationToken = default)
    {
        return await _context.ConnectorDefinitions
            .FirstOrDefaultAsync(cd => cd.ProviderType == providerType, cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ConnectorDefinitions.Where(cd => cd.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(cd => cd.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
