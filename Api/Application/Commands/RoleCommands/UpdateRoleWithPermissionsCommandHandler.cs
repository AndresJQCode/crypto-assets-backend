using System.Text.Json;
using Api.Application.Dtos.Permission;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.AuditAggregate;
using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.RoleAggregate;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Commands.RoleCommands;

internal sealed class UpdateRoleWithPermissionsCommandHandler(
        RoleManager<Role> roleManager,
        IPermissionRepository permissionRepository,
        IPermissionRoleRepository permissionRoleRepository,
        IPermissionService permissionService,
        ApiContext context,
        IIdentityService identityService
) : IRequestHandler<UpdateRoleWithPermissionsCommand, UpdateRoleWithPermissionsResponse>
{
    public async Task<UpdateRoleWithPermissionsResponse> Handle(UpdateRoleWithPermissionsCommand request, CancellationToken cancellationToken)
    {
        return await UpdateRoleWithPermissionsWithRetry(request, cancellationToken);
    }

    private async Task<UpdateRoleWithPermissionsResponse> UpdateRoleWithPermissionsWithRetry(
            UpdateRoleWithPermissionsCommand request,
            CancellationToken cancellationToken,
            int maxRetries = 3)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return await UpdateRoleWithPermissions(request, cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await HandleConcurrencyRetry(attempt, maxRetries, cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase) == true ||
                                                                                 ex.InnerException?.Message.Contains("optimistic", StringComparison.OrdinalIgnoreCase) == true ||
                                                                                 ex.InnerException?.Message.Contains("An error occurred while saving", StringComparison.OrdinalIgnoreCase) == true ||
                                                                                 ex.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase) == true ||
                                                                                 ex.Message.Contains("optimistic", StringComparison.OrdinalIgnoreCase) == true ||
                                                                                 ex.Message.Contains("An error occurred while saving", StringComparison.OrdinalIgnoreCase) == true)
            {
                await HandleConcurrencyRetry(attempt, maxRetries, cancellationToken);
            }
        }
        throw new InvalidOperationException("Error de concurrencia después de múltiples intentos");
    }

    private async Task<UpdateRoleWithPermissionsResponse> UpdateRoleWithPermissions(
            UpdateRoleWithPermissionsCommand request,
            CancellationToken cancellationToken)
    {
        // 1. Verificar que el rol existe
        Role role = await roleManager.FindByIdAsync(request.Id.ToString()) ?? throw new NotFoundException($"Rol con ID {request.Id} no encontrado");

        // 2. Validar que todos los permisos existen
        List<Guid>? newPermissionIds = request.PermissionIds?.ToList() ?? [];

        if (newPermissionIds.Count != 0)
        {
            var existingPermissions = await permissionRepository.GetByFilter(
                p => newPermissionIds.Contains(p.Id),
                cancellationToken: cancellationToken);

            var existingPermissionIds = existingPermissions.Select(p => p.Id).ToHashSet();
            var missingPermissionIds = newPermissionIds.Except(existingPermissionIds).ToList();

            if (missingPermissionIds.Count > 0)
            {
                throw new BadRequestException($"Permisos no encontrados: {string.Join(", ", missingPermissionIds)}");
            }
        }

        // 3. Obtener permisos anteriores antes de actualizar (para auditoría)
        var previousPermissionRoles = await permissionRoleRepository.GetByRoleIdAsync(role.Id, cancellationToken);
        var previousPermissionIds = previousPermissionRoles.Select(pr => pr.PermissionId).ToList();
        var permissionsToAdd = newPermissionIds.Except(previousPermissionIds).ToList();
        var permissionsToRemove = previousPermissionIds.Except(newPermissionIds).ToList();

        // 4. Actualizar el rol
        role.Name = request.Name;
        role.Description = request.Description;
        role.ConcurrencyStamp = Guid.NewGuid().ToString(); // Actualizar para evitar conflictos

        var roleUpdateResult = await roleManager.UpdateAsync(role);
        if (!roleUpdateResult.Succeeded)
        {
            if (roleUpdateResult.Errors.Any(e => e.Code == "ConcurrencyFailure"))
            {
                throw new DbUpdateConcurrencyException("El rol ha sido modificado por otro usuario. Por favor, recarga la página e intenta nuevamente.");
            }
            throw new InvalidOperationException($"Error al actualizar el rol: {string.Join(", ", roleUpdateResult.Errors.Select(e => e.Description))}");
        }

        // 5. Actualizar permisos del rol
        await UpdateRolePermissions(role.Id.ToString(), newPermissionIds, cancellationToken);

        // 6. Registrar en auditoría el cambio de permisos (solo si hay cambios)
        if (permissionsToAdd.Count > 0 || permissionsToRemove.Count > 0)
        {
            var currentUserId = identityService.GetCurrentUserId();
            var currentUserName = identityService.GetCurrentUserName();

            var additionalData = JsonSerializer.Serialize(new
            {
                RoleId = role.Id,
                RoleName = role.Name,
                PermissionsAdded = permissionsToAdd,
                PermissionsRemoved = permissionsToRemove,
                NewPermissionCount = newPermissionIds.Count,
                PreviousPermissionCount = previousPermissionIds.Count
            });

            var auditLog = new AuditLog(
                entityType: AppConstants.AuditEntityTypes.Role,
                entityId: role.Id,
                action: AppConstants.AuditActions.PermissionsUpdated,
                userId: currentUserId,
                userName: currentUserName,
                reason: $"Permisos actualizados para el rol {role.Name}",
                additionalData: additionalData
            );

            context.AuditLogs.Add(auditLog);
            await context.SaveEntitiesAsync(cancellationToken);
        }

        // 7. Obtener los permisos actualizados para la respuesta
        var currentPermissions = await permissionRoleRepository.GetPermissionsByRoleIdAsync(role.Id, cancellationToken);
        return new UpdateRoleWithPermissionsResponse
        {
            Id = role.Id.ToString(),
            Name = role.Name,
            Description = role.Description,
            Permissions = currentPermissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Resource = p.Resource,
                Action = p.Action,
                Description = p.Description
            }).ToList()
        };
    }

    private async Task UpdateRolePermissions(string roleId, List<Guid> newPermissionIds, CancellationToken cancellationToken)
    {
        Guid roleIdGuid = Guid.Parse(roleId);

        // Obtener todas las relaciones PermissionRole actuales del rol
        var currentPermissionRoles = await permissionRoleRepository.GetByRoleIdAsync(roleIdGuid, cancellationToken);
        List<Guid>? currentPermissionIds = currentPermissionRoles.Select(pr => pr.PermissionId).ToList();

        // Determinar qué permisos agregar y cuáles remover
        List<Guid>? permissionsToAdd = newPermissionIds.Except(currentPermissionIds).ToList();
        List<Guid>? permissionsToRemove = currentPermissionIds.Except(newPermissionIds).ToList();

        // Si no hay cambios, no hacer nada
        if (permissionsToAdd.Count == 0 && permissionsToRemove.Count == 0)
        {
            return;
        }

        // Procesar permisos a remover - eliminación física
        foreach (var permissionId in permissionsToRemove)
        {
            PermissionRole? permissionRole = currentPermissionRoles.FirstOrDefault(pr => pr.PermissionId == permissionId);
            if (permissionRole is not null)
            {
                permissionRoleRepository.Delete(permissionRole);
            }
        }

        // Procesar permisos a agregar - crear nuevas relaciones
        if (permissionsToAdd.Count != 0)
        {
            List<PermissionRole> permissionRolesToCreate = permissionsToAdd.Select(permissionId =>
                new PermissionRole(permissionId, roleIdGuid)).ToList();

            await permissionRoleRepository.CreateRange(permissionRolesToCreate, cancellationToken);
        }

        // Guardar todos los cambios
        await permissionRoleRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        // Invalidar caché de permisos solo para usuarios que tienen este rol
        await InvalidateCacheForRoleUsers(roleIdGuid, cancellationToken);
    }

    private static async Task HandleConcurrencyRetry(int attempt, int maxRetries, CancellationToken cancellationToken)
    {
        if (attempt == maxRetries - 1)
        {
            throw new InvalidOperationException("Error de concurrencia después de múltiples intentos");
        }
        // Esperar antes del siguiente intento
        await Task.Delay(100 * (attempt + 1), cancellationToken);
    }

    private async Task InvalidateCacheForRoleUsers(Guid roleId, CancellationToken cancellationToken)
    {
        // Obtener usuarios que tienen este rol específico usando consulta directa
        var userIds = await context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync(cancellationToken);

        // Invalidar caché solo para estos usuarios
        foreach (var userId in userIds)
        {
            await permissionService.InvalidateUserCacheAsync(userId);
        }
    }
}
