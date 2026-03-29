using Api.Application.Dtos.Auth;
using Api.Application.Services.Auth;
using MediatR;

namespace Api.Application.Commands.AuthCommands;

internal sealed class ExchangeCodeCommandHandler : IRequestHandler<ExchangeCodeCommand, ExchangeCodeResponseDto>
{
    private readonly IAuthProviderFactory _authProviderFactory;

    public ExchangeCodeCommandHandler(IAuthProviderFactory authProviderFactory)
    {
        _authProviderFactory = authProviderFactory;
    }

    public async Task<ExchangeCodeResponseDto> Handle(ExchangeCodeCommand request, CancellationToken cancellationToken)
    {
        // Validar que el proveedor sea compatible
        if (!_authProviderFactory.IsProviderSupported(request.Provider))
        {
            throw new ArgumentException($"El proveedor '{request.Provider}' no es compatible. Proveedores disponibles: Google, Microsoft");
        }

        // Obtener el servicio del proveedor específico
        var providerService = _authProviderFactory.GetProvider(request.Provider);

        // Intercambiar el código por un token y obtener información del usuario
        var result = await providerService.ExchangeCodeAsync(request.Code);

        return result;
    }
}
