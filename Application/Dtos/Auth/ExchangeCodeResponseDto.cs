namespace Application.Dtos.Auth;

internal sealed class ExchangeCodeResponseDto : LoginResponseDto
{
    public required string Provider { get; set; }
}
