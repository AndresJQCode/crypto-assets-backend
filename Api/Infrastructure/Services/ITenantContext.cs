namespace Api.Infrastructure.Services;

/// <summary>
/// Provee el tenant actual del request. Null = usuario de plataforma (SuperAdmin) que puede ver todos los tenants.
/// </summary>
internal interface ITenantContext
{
    Guid? GetCurrentTenantId();
}
