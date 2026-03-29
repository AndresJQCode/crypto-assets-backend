using Api.Application.Dtos.User;
using Api.Application.Queries.Users;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using Infrastructure;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Queries.UsersQueries;

internal sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponseDto>
{
    private readonly UserManager<User> _userManager;
    private readonly ApiContext _context;

    public GetUserByIdQueryHandler(UserManager<User> userManager, ApiContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<UserResponseDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        User? user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken) ?? throw new NotFoundException($"Usuario con ID {request.Id} no encontrado.");

        UserRoleDto[] userRoles = await (from ur in _context.UserRoles
                                         join r in _context.Roles on ur.RoleId equals r.Id
                                         where ur.UserId == user.Id
                                         select new UserRoleDto
                                         {
                                             Id = r.Id.ToString(),
                                             Name = r.Name ?? string.Empty,
                                         }).ToArrayAsync(cancellationToken);

        UserResponseDto userDto = user.Adapt<UserResponseDto>();
        userDto.Roles = userRoles;
        return userDto;
    }
}
