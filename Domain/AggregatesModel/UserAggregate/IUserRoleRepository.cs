namespace Domain.AggregatesModel.UserAggregate;

/// <summary>
/// Repositorio para consultas sobre la relación usuario-rol (Identity UserRole).
/// </summary>
public interface IUserRoleRepository
{
    /// <summary>
    /// Obtiene los IDs de usuario que tienen asignado el rol indicado.
    /// </summary>
    Task<List<Guid>> GetUserIdsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
}
