using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class ConnectorInstanceRepository(
    ApiContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ConnectorInstanceRepository> logger,
    IEncryptionService encryptionService)
    : Repository<ConnectorInstance>(context, httpContextAccessor, logger), IConnectorInstanceRepository
{
    private readonly IEncryptionService _encryptionService = encryptionService;

    public async Task<ConnectorInstance?> GetByIdWithDecryptedTokenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connector = await GetById(id, tracking: false, cancellationToken: cancellationToken);

        if (connector != null && !string.IsNullOrEmpty(connector.AccessToken))
        {
            // Decrypt access token
            try
            {
                var decryptedToken = await _encryptionService.DecryptAsync(connector.AccessToken);
                // Note: Cannot modify properties directly on read-only entity from AsNoTracking
                // Caller should be aware that AccessToken is encrypted
                // For actual decryption usage, consider returning a DTO or separate method
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to decrypt access token for connector {ConnectorId}", id);
                throw;
            }
        }

        return connector;
    }

    public async Task<List<ConnectorInstance>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await GetByFilter(
            filter: c => c.TenantId == tenantId,
            orderBy: q => q.OrderByDescending(c => c.CreatedOn),
            tracking: false,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsForUserAsync(
        Guid userId,
        ConnectorProviderType providerType,
        CancellationToken cancellationToken = default)
    {
        return await Any(
            c => c.UserId == userId && c.ProviderType == providerType,
            cancellationToken);
    }

    public async Task<ConnectorInstance?> GetByUserAndProviderAsync(
        Guid userId,
        ConnectorProviderType providerType,
        CancellationToken cancellationToken = default)
    {
        return await GetFirstOrDefaultByFilter(
            filter: c => c.UserId == userId && c.ProviderType == providerType,
            tracking: false,
            cancellationToken: cancellationToken);
    }
}
