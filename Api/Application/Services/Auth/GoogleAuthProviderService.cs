using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Interfaces;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Api.Application.Services.Auth;

/// <summary>
/// Adaptador para convertir GoogleUserInfo a IExternalUserInfo
/// </summary>
internal sealed class GoogleUserInfoAdapter : IExternalUserInfo
{
    private readonly GoogleUserInfo _userInfo;

    public GoogleUserInfoAdapter(GoogleUserInfo userInfo)
    {
        _userInfo = userInfo;
    }

    public string GetEmail() => _userInfo.Email;
    public string? GetName() => _userInfo.Name;
    public bool GetEmailConfirmed() => _userInfo.VerifiedEmail;
}

internal sealed class GoogleAuthProviderService : BaseAuthProviderService
{
    private readonly IGoogleOAuthService _googleOAuthService;

    public GoogleAuthProviderService(
        IGoogleOAuthService googleOAuthService,
        IJwtTokenService jwtTokenService,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IUserInfoService userInfoService,
        IOptions<OAuthSettings> oAuthSettings,
        ILogger<GoogleAuthProviderService> logger)
        : base(jwtTokenService, userManager, roleManager, userInfoService, oAuthSettings, logger)
    {
        _googleOAuthService = googleOAuthService;
    }

    public override string ProviderName => "Google";

    protected override async Task<string> ExchangeCodeForAccessTokenAsync(string code)
    {
        var tokenResponse = await _googleOAuthService.ExchangeCodeForTokenAsync(code);
        return tokenResponse.AccessToken;
    }

    protected override async Task<IExternalUserInfo> GetExternalUserInfoAsync(string accessToken)
    {
        var userInfo = await _googleOAuthService.GetUserInfoAsync(accessToken);
        return new GoogleUserInfoAdapter(userInfo);
    }

    protected override UserInfoMapping MapExternalUserInfoToUser(IExternalUserInfo externalUserInfo)
    {
        return new UserInfoMapping
        {
            Email = externalUserInfo.GetEmail(),
            Name = externalUserInfo.GetName(),
            EmailConfirmed = externalUserInfo.GetEmailConfirmed(),
            NormalizedUserName = externalUserInfo.GetEmail().ToUpperInvariant()
        };
    }

    // Google requiere validación específica de usuario activo
    protected override async Task ValidateUserBeforeAuthenticationAsync(User user)
    {
        await base.ValidateUserBeforeAuthenticationAsync(user);
    }
}
