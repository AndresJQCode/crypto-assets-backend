using Domain.SeedWork;

namespace Domain.AggregatesModel.PermissionAggregate
{
    public interface IPermissionRepository : IRepository<Permission>
    {
        Task<IEnumerable<Permission>> GetByResourceAsync(string resource);
        Task<IEnumerable<Permission>> GetActivePermissionsAsync();
        Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(Guid userId);
    }
}
