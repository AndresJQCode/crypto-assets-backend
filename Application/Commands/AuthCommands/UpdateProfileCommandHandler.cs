using Application.Dtos.Auth;
using Application.Services.Auth;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.AuthCommands;

internal sealed class UpdateProfileCommandHandler(
        UserManager<User> userManager,
        IUserInfoService userInfoService,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<UpdateProfileCommand, AuthUserDto>
{
    public async Task<AuthUserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        string? userId = httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
        {
            throw new UnAuthorizedException("Usuario no autenticado");
        }

        var existingUser = await userManager.FindByIdAsync(userId) ?? throw new NotFoundException("Usuario no encontrado");

        existingUser.Name = request.Name;

        IdentityResult result = await userManager.UpdateAsync(existingUser);
        if (!result.Succeeded)
        {
            string? errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Error al actualizar el perfil: {errors}");
        }

        return await userInfoService.GetUserInfoAsync(existingUser);

    }
}
