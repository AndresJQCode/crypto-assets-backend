using Infrastructure.Services.Auth.Dtos;

namespace Infrastructure.Services.Auth;

public interface IMicrosoftOAuthService
{
    Task<MicrosoftTokenResponse> ExchangeCodeForTokenAsync(string code);
    Task<MicrosoftUserInfo> GetUserInfoAsync(string accessToken);
}
