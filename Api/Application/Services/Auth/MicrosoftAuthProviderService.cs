using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Interfaces;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Auth.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Application.Services.Auth;

/// <summary>
/// Adaptador para convertir MicrosoftUserInfo a IExternalUserInfo
/// </summary>
internal sealed class MicrosoftUserInfoAdapter : IExternalUserInfo
{
    private readonly MicrosoftUserInfo _userInfo;

    public MicrosoftUserInfoAdapter(MicrosoftUserInfo userInfo)
    {
        _userInfo = userInfo;
    }

    public string GetEmail() => _userInfo.Mail;
    public string? GetName() => _userInfo.DisplayName;
    public bool GetEmailConfirmed() => true; // Microsoft Graph usuarios son verificados
}

internal sealed class MicrosoftAuthProviderService : BaseAuthProviderService
{
    private readonly IMicrosoftOAuthService _microsoftOAuthService;

    public MicrosoftAuthProviderService(
        IMicrosoftOAuthService microsoftOAuthService,
        IJwtTokenService jwtTokenService,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IUserInfoService userInfoService,
        IOptions<OAuthSettings> oAuthSettings,
        ILogger<MicrosoftAuthProviderService> logger)
        : base(jwtTokenService, userManager, roleManager, userInfoService, oAuthSettings, logger)
    {
        _microsoftOAuthService = microsoftOAuthService;
    }

    public override string ProviderName => "Microsoft";

    protected override async Task<string> ExchangeCodeForAccessTokenAsync(string code)
    {
        var tokenResponse = await _microsoftOAuthService.ExchangeCodeForTokenAsync(code);
        return tokenResponse.AccessToken;
    }

    protected override async Task<IExternalUserInfo> GetExternalUserInfoAsync(string accessToken)
    {
        var userInfo = await _microsoftOAuthService.GetUserInfoAsync(accessToken);
        return new MicrosoftUserInfoAdapter(userInfo);
    }

    protected override UserInfoMapping MapExternalUserInfoToUser(IExternalUserInfo externalUserInfo)
    {
        return new UserInfoMapping
        {
            Email = externalUserInfo.GetEmail(),
            Name = externalUserInfo.GetName(),
            EmailConfirmed = externalUserInfo.GetEmailConfirmed(),
            NormalizedUserName = externalUserInfo.GetName()?.ToUpperInvariant() ?? externalUserInfo.GetEmail().ToUpperInvariant()
        };
    }
}
