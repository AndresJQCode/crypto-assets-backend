using Application.Dtos.User;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.UserCommands;

internal sealed class UpdateUserCommandHandler(UserManager<User> userManager, RoleManager<Role> roleManager, IPermissionService permissionService) : IRequestHandler<UpdateUserCommand, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Buscar el usuario
        var user = await userManager.FindByIdAsync(request.Id.ToString());
        if (user == null)
        {
            throw new NotFoundException($"Usuario con ID {request.Id} no encontrado");
        }

        // Verificar si el email ya existe en otro usuario
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null && existingUser.Id != request.Id)
        {
            throw new BadRequestException("Ya existe otro usuario con ese email");
        }

        // Actualizar propiedades básicas del usuario
        user.Name = request.Name;
        user.Email = request.Email;
        user.UserName = request.Email; // UserName debe coincidir con Email

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Error al actualizar el usuario: {errors}");
        }

        // Actualizar roles si se proporcionan
        if (request.RoleIds != null)
        {
            await UpdateUserRoles(user, request.RoleIds.ToList());
        }

        // Obtener los roles actuales del usuario para la respuesta
        var userRoles = await userManager.GetRolesAsync(user);
        var roleDtos = new List<UserRoleDto>();

        foreach (var roleName in userRoles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                roleDtos.Add(new UserRoleDto
                {
                    Id = role.Id.ToString(),
                    Name = role.Name!
                });
            }
        }

        return new UserResponseDto
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            Name = user.Name,
            Roles = roleDtos.ToArray()
        };
    }

    private async Task UpdateUserRoles(User user, List<string> roleIds)
    {
        // Obtener roles actuales del usuario
        var currentRoles = await userManager.GetRolesAsync(user);
        var currentRoleNames = currentRoles.ToList();

        // Obtener los roles que se van a asignar
        var newRoleNames = new List<string>();

        foreach (var roleIdString in roleIds)
        {
            if (!Guid.TryParse(roleIdString, out var roleId))
            {
                throw new BadRequestException($"ID de rol inválido: {roleIdString}");
            }

            var role = await roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                throw new BadRequestException($"Rol con ID {roleIdString} no encontrado");
            }
            newRoleNames.Add(role.Name!);
        }

        // Determinar qué roles agregar y cuáles remover
        var rolesToAdd = newRoleNames.Except(currentRoleNames).ToList();
        var rolesToRemove = currentRoleNames.Except(newRoleNames).ToList();

        // Remover roles que ya no están en la lista
        foreach (var roleName in rolesToRemove)
        {
            var result = await RemoveUserFromRoleWithRetry(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Error al remover el rol {roleName}: {errors}");
            }
        }

        // Agregar nuevos roles
        foreach (var roleName in rolesToAdd)
        {
            var result = await userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Error al asignar el rol {roleName}: {errors}");
            }
        }

        if (rolesToAdd.Count > 0 || rolesToRemove.Count > 0)
        {
            await permissionService.InvalidateUserCacheAsync(user.Id);
        }
    }

    private async Task<IdentityResult> RemoveUserFromRoleWithRetry(User user, string roleName, int maxRetries = 3)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                // Recargar el usuario para obtener la versión más reciente
                var refreshedUser = await userManager.FindByIdAsync(user.Id.ToString());
                if (refreshedUser == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado" });
                }
                user = refreshedUser;

                var result = await userManager.RemoveFromRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    return result;
                }

                // Si no es un error de concurrencia, no reintentar
                if (!result.Errors.Any(e => e.Description.Contains("concurrency", StringComparison.OrdinalIgnoreCase) || e.Description.Contains("optimistic", StringComparison.OrdinalIgnoreCase)))
                {
                    return result;
                }

                // Esperar un poco antes del siguiente intento
                if (attempt < maxRetries - 1)
                {
                    await Task.Delay(100 * (attempt + 1)); // Delay incremental
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si es el último intento, relanzar la excepción
                if (attempt == maxRetries - 1)
                {
                    throw;
                }
                // Esperar antes del siguiente intento
                await Task.Delay(100 * (attempt + 1));
            }
        }

        return IdentityResult.Failed(new IdentityError { Description = "Error de concurrencia después de múltiples intentos" });
    }

}
