using Api.Application.Dtos.SystemConfiguration;
using MediatR;

namespace Api.Application.Queries.SystemConfigurationQueries;

public record GetSystemConfigurationByKeyQuery(string Key) : IRequest<SystemConfigurationDto?>;
