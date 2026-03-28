namespace Api.Infrastructure.Services
{
    internal interface IPermissionService
    {
        Task<bool> HasPermissionAsync(Guid userId, string resource, string action);
        Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
        Task<bool> HasAnyPermissionAsync(Guid userId, params string[] permissionKeys);
        Task<bool> HasAllPermissionsAsync(Guid userId, params string[] permissionKeys);
        Task InvalidateUserCacheAsync(Guid userId);
        Task InvalidateAllCacheAsync();
    }
}
