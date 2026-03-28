using MediatR;

namespace Api.Application.Commands.SystemConfigurationCommands;

public record ToggleSystemConfigurationCommand(
    string Key,
    bool IsActive
) : IRequest<bool>;
