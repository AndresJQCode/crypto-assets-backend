namespace Api.Application.Dtos.SystemConfiguration;

public record UpdateSystemConfigurationDto(
    string Value,
    string? Description
);
