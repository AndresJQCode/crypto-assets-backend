using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;

namespace Api.Application.Services.Auth;

internal sealed class MicrosoftAuthProviderService(
    IMicrosoftOAuthService microsoftOAuthService,
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    ILogger<MicrosoftAuthProviderService> logger) : BaseAuthProviderService(userManager, roleManager, logger)
{
    public override string ProviderName => "Microsoft";

    protected override async Task<string> ExchangeCodeForAccessTokenAsync(string code)
    {
        var tokenResponse = await microsoftOAuthService.ExchangeCodeForTokenAsync(code);
        return tokenResponse.AccessToken;
    }

    protected override async Task<ExternalUserInfo> GetExternalUserWithAccessTokenAsync(string accessToken)
    {
        var userInfo = await microsoftOAuthService.GetUserInfoAsync(accessToken);
        return new ExternalUserInfo
        {
            Email = userInfo.Mail,
            Name = userInfo.DisplayName,
            EmailConfirmed = true // Microsoft Graph usuarios son verificados
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
}
