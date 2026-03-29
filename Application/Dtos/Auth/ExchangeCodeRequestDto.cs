namespace Application.Dtos.Auth;

internal sealed class ExchangeCodeRequestDto
{
    public required string Code { get; set; }
    public required string Provider { get; set; }
}
