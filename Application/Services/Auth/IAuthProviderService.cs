
using Application.Dtos.Auth;

namespace Application.Services.Auth;

internal interface IAuthProviderService
{
    string ProviderName { get; }
    Task<ExchangeCodeResponseDto> ExchangeCodeAsync(string code);
}
