using Application.Dtos.Auth;
using Application.Dtos.Permission;
using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.UserAggregate;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Auth;

internal sealed class UserInfoService(UserManager<User> userManager, IPermissionRepository permissionRepository) : IUserInfoService
{
    public async Task<AuthUserDto> GetUserInfoAsync(User user)
    {
        // Obtener roles del usuario
        IList<string>? roles = await userManager.GetRolesAsync(user) ?? [];

        IEnumerable<Permission>? permissions = await permissionRepository.GetPermissionsByUserIdAsync(user.Id);

        IEnumerable<UserPermissionDto>? permissionsDto = permissions.Select(p => new UserPermissionDto
        {
            PermissionKey = p.PermissionKey,
            Resource = p.Resource,
            Action = p.Action
        });

        return new AuthUserDto
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? user.UserName ?? string.Empty,
            Roles = [.. roles],
            Permissions = [.. permissionsDto]
        };
    }
}
