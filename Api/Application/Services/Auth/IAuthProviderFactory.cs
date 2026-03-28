namespace Api.Application.Services.Auth;

internal interface IAuthProviderFactory
{
    IAuthProviderService GetProvider(string providerName);
    bool IsProviderSupported(string providerName);
}
