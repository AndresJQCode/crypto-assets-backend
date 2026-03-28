using System;

namespace Api.Application.Services.Auth;

internal sealed class AuthProviderFactory : IAuthProviderFactory
{
    private readonly IEnumerable<IAuthProviderService> _providers;

    public AuthProviderFactory(IEnumerable<IAuthProviderService> providers)
    {
        _providers = providers;
    }

    public IAuthProviderService GetProvider(string providerName)
    {
        var provider = _providers.FirstOrDefault(p =>
                string.Equals(p.ProviderName, providerName, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            throw new ArgumentException($"Proveedor de autenticación '{providerName}' no es compatible.");
        }

        return provider;
    }

    public bool IsProviderSupported(string providerName)
    {
        return _providers.Any(p =>
                string.Equals(p.ProviderName, providerName, StringComparison.OrdinalIgnoreCase));
    }
}
