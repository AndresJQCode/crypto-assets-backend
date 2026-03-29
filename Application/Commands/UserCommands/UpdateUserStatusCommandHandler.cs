using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.UserCommands;

internal sealed class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, bool>
{
    private readonly UserManager<User> _userManager;

    public UpdateUserStatusCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken) ?? throw new NotFoundException("Usuario no encontrado");

        // Actualizar el estado del usuario
        user.SetActive(request.IsActive);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new SaveEntitiesException("Error al actualizar el estado del usuario");
        }

        return true;
    }
}
