using Domain.AggregatesModel.UserAggregate;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.AuthCommands;

internal sealed class LogoutCommandHandler(UserManager<User> userManager) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id);

        if (user is null)
        {
            return;
        }
        await userManager.RemoveAuthenticationTokenAsync(user, AppConstants.Authentication.DefaultProvider, AppConstants.Authentication.AccessTokenName);
        // eliminar el refresh token
        await userManager.RemoveAuthenticationTokenAsync(user, AppConstants.Authentication.DefaultProvider, AppConstants.Authentication.RefreshTokenName);
        return;
    }
}
