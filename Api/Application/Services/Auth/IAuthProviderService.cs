namespace Api.Application.Services.Auth;

internal interface IAuthProviderService
{
    string ProviderName { get; }
    Task<string> ExchangeCodeAsync(string code);
    Task<ExternalUserInfo> GetExternalUserInfoAsync(string accessToken);
    UserInfoMapping MapExternalUserInfoToUser(ExternalUserInfo externalUserInfo);
}
