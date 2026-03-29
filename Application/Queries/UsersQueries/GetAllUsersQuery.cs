using Application.Dtos;
using Application.Dtos.User;
using Api.Utilities;
using MediatR;

namespace Application.Queries.UsersQueries;

internal sealed class GetAllUsersQuery : IRequest<PaginationResponseDto<UserResponseDto>>
{
    public PaginationParameters PaginationParameters { get; set; } = new();
}
