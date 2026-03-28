using MediatR;

namespace Api.Application.Commands.SystemConfigurationCommands;

public record UpdateSystemConfigurationCommand(
    string Key,
    string Value,
    string? Description
) : IRequest<bool>;
