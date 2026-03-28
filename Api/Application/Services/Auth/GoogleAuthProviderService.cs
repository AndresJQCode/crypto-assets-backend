using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;

namespace Api.Application.Services.Auth;

internal sealed class GoogleAuthProviderService(
    IGoogleOAuthService googleOAuthService,
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    ILogger<GoogleAuthProviderService> logger) : BaseAuthProviderService(userManager, roleManager, logger)
{
    public override string ProviderName => "Google";

    protected override async Task<string> ExchangeCodeForAccessTokenAsync(string code)
    {
        var tokenResponse = await googleOAuthService.ExchangeCodeForTokenAsync(code);
        return tokenResponse.AccessToken;
    }

    protected override async Task<ExternalUserInfo> GetExternalUserWithAccessTokenAsync(string accessToken)
    {
        var userInfo = await googleOAuthService.GetUserInfoAsync(accessToken);
        return new ExternalUserInfo
        {
            Email = userInfo.Email,
            Name = userInfo.Name,
            EmailConfirmed = userInfo.VerifiedEmail
        };
    }

    public override UserInfoMapping MapExternalUserInfoToUser(ExternalUserInfo externalUserInfo)
    {
        return new UserInfoMapping
        {
            Email = externalUserInfo.Email,
            Name = externalUserInfo.Name,
            EmailConfirmed = externalUserInfo.EmailConfirmed,
        };
    }

    // Google requiere validación específica de usuario activo
    protected override async Task ValidateUserBeforeAuthenticationAsync(User user)
    {
        await base.ValidateUserBeforeAuthenticationAsync(user);
    }
}
