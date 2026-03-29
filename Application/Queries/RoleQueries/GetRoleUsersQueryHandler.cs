using Application.Dtos.User;
using Domain.AggregatesModel.UserAggregate;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.RoleQueries
{
    internal sealed class GetRoleUsersQueryHandler : IRequestHandler<GetRoleUsersQuery, IEnumerable<UserResponseDto>>
    {
        private readonly ApiContext _context;
        private readonly UserManager<User> _userManager;

        public GetRoleUsersQueryHandler(ApiContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserResponseDto>> Handle(GetRoleUsersQuery request, CancellationToken cancellationToken)
        {
            // Obtener usuarios que tienen el rol específico
            var users = await (from ur in _context.UserRoles
                               join u in _context.Users on ur.UserId equals u.Id
                               where ur.RoleId == request.RoleId
                               select u).ToListAsync(cancellationToken);

            var userDtos = new List<UserResponseDto>();

            foreach (var user in users)
            {
                // Obtener roles del usuario
                var userRoles = await (from ur in _context.UserRoles
                                       join r in _context.Roles on ur.RoleId equals r.Id
                                       where ur.UserId == user.Id
                                       select new UserRoleDto
                                       {
                                           Id = r.Id.ToString(),
                                           Name = r.Name ?? string.Empty,
                                       }).ToArrayAsync(cancellationToken);

                var userDto = new UserResponseDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email ?? string.Empty,
                    Name = user.Name ?? user.UserName ?? string.Empty,
                    IsActive = user.IsActive,
                    Roles = userRoles
                };

                userDtos.Add(userDto);
            }

            return userDtos;
        }
    }
}
