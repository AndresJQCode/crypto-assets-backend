
using Api.Application.Dtos.Auth;

namespace Api.Application.Services.Auth;

internal interface IAuthProviderService
{
    string ProviderName { get; }
    Task<ExchangeCodeResponseDto> ExchangeCodeAsync(string code);
}
