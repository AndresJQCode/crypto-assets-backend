using Domain.AggregatesModel.RoleAggregate;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Commands.RoleCommands
{
    internal sealed class RemoveUserFromRoleCommandHandler : IRequestHandler<RemoveUserFromRoleCommand, bool>
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<Domain.AggregatesModel.UserAggregate.User> _userManager;

        public RemoveUserFromRoleCommandHandler(RoleManager<Role> roleManager, UserManager<Domain.AggregatesModel.UserAggregate.User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<bool> Handle(RemoveUserFromRoleCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el usuario existe
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                throw new NotFoundException($"Usuario con ID {request.UserId} no encontrado");
            }

            // Verificar que el rol existe
            var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
            if (role == null)
            {
                throw new NotFoundException($"Rol con ID {request.RoleId} no encontrado");
            }

            // Verificar si el usuario tiene el rol
            var isInRole = await _userManager.IsInRoleAsync(user, role.Name!);
            if (!isInRole)
            {
                throw new InvalidOperationException("El usuario no tiene asignado este rol");
            }

            // Remover el rol del usuario usando UserManager con reintentos
            var result = await RemoveUserFromRoleWithRetry(user, role.Name!);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Error al remover el rol: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return true;
        }

        private async Task<IdentityResult> RemoveUserFromRoleWithRetry(Domain.AggregatesModel.UserAggregate.User user, string roleName, int maxRetries = 3)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Recargar el usuario para obtener la versión más reciente
                    var refreshedUser = await _userManager.FindByIdAsync(user.Id.ToString());
                    if (refreshedUser == null)
                    {
                        return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado" });
                    }
                    user = refreshedUser;

                    var result = await _userManager.RemoveFromRoleAsync(user, roleName);
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
}
