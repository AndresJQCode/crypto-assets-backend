using Domain.AggregatesModel.TenantAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class TenantRepository(
    ApiContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<TenantRepository> logger)
    : Repository<Tenant>(context, httpContextAccessor, logger), ITenantRepository
{
    private readonly ApiContext _context = context;

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug, ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.Tenants.Where(t => t.Slug == slug);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }
}
