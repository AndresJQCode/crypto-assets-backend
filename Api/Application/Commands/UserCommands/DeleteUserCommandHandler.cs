using System.Security.Claims;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using Domain.SeedWork;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Commands.UserCommands;

internal sealed class DeleteUserCommandHandler(
    UserManager<User> userManager,
    IAuditTrail auditTrail,
    ILogger<DeleteUserCommandHandler> logger,
    IHttpContextAccessor httpContextAccessor,
    ITenantContext tenantContext) : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var claimsPrincipal = httpContextAccessor.HttpContext?.User;
            var currentUserId = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserIdGuid))
                throw new UnAuthorizedException("Usuario no autenticado");
            var currentUserName = claimsPrincipal?.FindFirst(ClaimTypes.Name)?.Value ?? claimsPrincipal?.Identity?.Name ?? "Usuario";

            User? user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (user == null)
            {
                logger.LogWarning("Usuario con ID {UserId} no encontrado", request.Id);
                throw new NotFoundException("Usuario no encontrado");
            }
            if (tenantContext.GetCurrentTenantId() is { } tenantId && user.TenantId != tenantId)
                throw new NotFoundException("Usuario no encontrado");

            // Quitar al usuario de todos los roles antes de eliminar para evitar que queden registros en UserRoles
            // (por si la eliminación en cascada no se aplica correctamente con Identity)
            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Count > 0)
            {
                var removeFromRolesResult = await userManager.RemoveFromRolesAsync(user, userRoles);
                if (!removeFromRolesResult.Succeeded)
                {
                    logger.LogError("Error al quitar roles del usuario {UserId} antes de eliminar", request.Id);
                    throw new SaveEntitiesException("Error al quitar los roles del usuario antes de eliminar");
                }
            }

            // Capturar snapshot del usuario antes de eliminarlo
            var userSnapshot = new
            {
                user.Id,
                user.Email,
                user.UserName,
                user.Name,
                user.PhoneNumber,
                user.EmailConfirmed,
                user.IsActive,
                Roles = userRoles
            };

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                logger.LogError("Error al eliminar usuario con ID {UserId}", request.Id);
                throw new SaveEntitiesException("Error al eliminar el usuario");
            }

            // Registrar en auditoría con snapshot de los datos del usuario
            await auditTrail.LogDeletionAsync(
                entityType: nameof(User),
                entityId: user.Id,
                deletedBy: currentUserIdGuid,
                deletedByName: currentUserName,
                reason: request.Reason,
                entitySnapshot: userSnapshot);

            logger.LogInformation("Usuario con ID {UserId} eliminado exitosamente", request.Id);
            return true;
        }
        catch (Exception ex) when (!(ex is NotFoundException || ex is InvalidOperationException || ex is SaveEntitiesException))
        {
            logger.LogError(ex, "Error inesperado al eliminar usuario con ID {UserId}", request.Id);
            throw;
        }
    }
}
