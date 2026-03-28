namespace Api.Application.Dtos.SystemConfiguration;

public record SystemConfigurationDto(
    Guid Id,
    string Key,
    string Value,
    string? Description,
    bool IsActive,
    DateTimeOffset? LastModifiedOn,
    string? LastModifiedByName
);
