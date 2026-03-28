using Api.Application.Dtos.Role;
using Domain.AggregatesModel.UserAggregate;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Api.Application.Queries.RoleQueries
{
    internal sealed class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, IEnumerable<RoleDto>>
    {
        private readonly UserManager<User> _userManager;

        public GetUserRolesQueryHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<RoleDto>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return new List<RoleDto>();
            }

            var roleNames = await _userManager.GetRolesAsync(user);

            // Convertir nombres de roles a RoleDto
            return roleNames.Select((roleName, index) => new RoleDto
            {
                Id = Guid.NewGuid().ToString(), // Generar un GUID temporal para el ID
                Name = roleName,
            });
        }
    }
}
