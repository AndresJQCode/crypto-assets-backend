using Domain.AggregatesModel.AuditAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class AuditLogRepository(
    ApiContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<Repository<AuditLog>> logger)
    : Repository<AuditLog>(context, httpContextAccessor, logger), IAuditLogRepository
{
}
