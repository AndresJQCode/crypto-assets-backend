using Domain.AggregatesModel.RoleAggregate;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.RoleCommands
{
    internal sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
    {
        private readonly RoleManager<Role> _roleManager;

        public DeleteRoleCommandHandler(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.Id.ToString());

            if (role == null)
            {
                throw new NotFoundException($"Rol con ID {request.Id} no encontrado");
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Error al eliminar el rol: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return true;
        }
    }
}
