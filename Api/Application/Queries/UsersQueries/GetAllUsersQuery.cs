using Api.Application.Dtos;
using Api.Application.Dtos.User;
using Api.Application.Queries;
using Api.Utilities;
using MediatR;

namespace Api.Application.Queries.UsersQueries;

internal sealed class GetAllUsersQuery
    : IRequest<PaginationResponseDto<UserResponseDto>>,
      IPaginatedQuery<PaginationResponseDto<UserResponseDto>>
{
    public PaginationParameters PaginationParameters { get; set; } = new();
}
