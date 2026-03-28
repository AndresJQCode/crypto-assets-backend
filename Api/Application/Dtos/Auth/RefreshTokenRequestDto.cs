namespace Api.Application.Dtos.Auth;

internal sealed class RefreshTokenRequestDto
{
    public required string RefreshToken { get; set; }
}
