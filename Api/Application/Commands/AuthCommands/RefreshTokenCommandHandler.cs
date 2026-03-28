using Api.Application.Dtos.Auth;
using Api.Application.Services.Auth;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Api.Application.Commands.AuthCommands;

internal sealed class RefreshTokenCommandHandler(
    UserManager<User> userManager,
    IJwtTokenService jwtTokenService,
    IUserInfoService userInfoService) : IRequestHandler<RefreshTokenCommand, LoginResponseDto>
{
    public async Task<LoginResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Validar el refresh token
        var principal = jwtTokenService.ValidateRefreshToken(request.RefreshToken);
        if (principal == null)
        {
            throw new UnAuthorizedException("Refresh token inválido");
        }

        // Obtener el ID del usuario del token
        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            throw new UnAuthorizedException("Token inválido");
        }

        // Buscar el usuario
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new UnAuthorizedException("Usuario no encontrado");
        }

        // Verificar que el refresh token almacenado coincida
        var storedRefreshToken = await userManager.GetAuthenticationTokenAsync(user, AppConstants.Authentication.DefaultProvider, AppConstants.Authentication.RefreshTokenName);
        if (storedRefreshToken != request.RefreshToken)
        {
            throw new UnAuthorizedException("Refresh token no coincide");
        }

        // Generar nuevos tokens
        var newAccessToken = jwtTokenService.GenerateToken(user, AppConstants.Authentication.DefaultProvider);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken(user);

        // Actualizar tokens en el usuario
        var resultSetAccessToken = await userManager.SetAuthenticationTokenAsync(user, AppConstants.Authentication.DefaultProvider, AppConstants.Authentication.AccessTokenName, newAccessToken);
        var resultSetRefreshToken = await userManager.SetAuthenticationTokenAsync(user, AppConstants.Authentication.DefaultProvider, AppConstants.Authentication.RefreshTokenName, newRefreshToken);

        if (!resultSetAccessToken.Succeeded || !resultSetRefreshToken.Succeeded)
        {
            var errors = string.Join(", ", resultSetAccessToken.Errors.Concat(resultSetRefreshToken.Errors).Select(e => e.Description));
            throw new DomainException($"Error al actualizar tokens: {errors}");
        }

        // Obtener información completa del usuario (roles y permisos)
        var userInfo = await userInfoService.GetUserInfoAsync(user);

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            User = userInfo
        };
    }
}
