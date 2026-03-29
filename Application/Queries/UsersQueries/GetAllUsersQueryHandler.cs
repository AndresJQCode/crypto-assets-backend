using System.Globalization;
using System.Linq;
using Application.Dtos;
using Application.Dtos.User;
using Api.Utilities;
using Domain.AggregatesModel.UserAggregate;
using Infrastructure;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.UsersQueries;

internal sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PaginationResponseDto<UserResponseDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApiContext _context;

    public GetAllUsersQueryHandler(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, ApiContext context)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public async Task<PaginationResponseDto<UserResponseDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        PaginationParameters? paginationParameters = request.PaginationParameters;

        Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<User, ICollection<UserRole>>? baseQuery = _userManager.Users
            .Include(u => u.UserRoles);

        // Aplicar filtro de búsqueda si se proporciona
        IQueryable<User> filteredQuery = baseQuery;
        if (!string.IsNullOrEmpty(paginationParameters.Search))
        {
            string? searchTerm = paginationParameters.Search.ToUpperInvariant();
            filteredQuery = baseQuery.Where(u =>
                u.Name != null && u.Name.ToUpperInvariant().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Email != null && u.Email.ToUpperInvariant().Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Aplicar ordenamiento
        IQueryable<User> query;
        if (!string.IsNullOrEmpty(paginationParameters.SortBy))
        {
            var sortByNormalized = paginationParameters.SortBy.ToUpperInvariant();
            var isDescending = paginationParameters.SortOrder?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;

            query = sortByNormalized switch
            {
                "name" => isDescending ? filteredQuery.OrderByDescending(u => u.Name) : filteredQuery.OrderBy(u => u.Name),
                "email" => isDescending ? filteredQuery.OrderByDescending(u => u.Email) : filteredQuery.OrderBy(u => u.Email),
                "isactive" => isDescending ? filteredQuery.OrderByDescending(u => u.IsActive) : filteredQuery.OrderBy(u => u.IsActive),
                _ => filteredQuery.OrderBy(u => u.Name) // Ordenamiento por defecto
            };
        }
        else
        {
            query = filteredQuery.OrderBy(u => u.Name); // Ordenamiento por defecto
        }

        int totalCount = await query.CountAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling(totalCount / (double)paginationParameters.Limit);

        List<User>? users = await query
            .Skip((paginationParameters.Page - 1) * paginationParameters.Limit)
            .Take(paginationParameters.Limit)
            .ToListAsync(cancellationToken);

        // Optimización: Obtener todos los roles de usuarios en una sola consulta
        var userIds = users.Select(u => u.Id).ToList();
        var userRolesDict = await (from ur in _context.UserRoles
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   where userIds.Contains(ur.UserId)
                                   select new { ur.UserId, Role = new UserRoleDto { Id = r.Id.ToString(), Name = r.Name ?? string.Empty } })
                                  .GroupBy(x => x.UserId)
                                  .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.Role).ToArray(), cancellationToken);

        List<UserResponseDto>? usersDto = [];
        foreach (var user in users)
        {
            var userDto = user.Adapt<UserResponseDto>();
            userDto.Roles = userRolesDict.GetValueOrDefault(user.Id, []);
            usersDto.Add(userDto);
        }

        return new PaginationResponseDto<UserResponseDto>
        {
            Data = usersDto,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Limit = paginationParameters.Limit,
            Page = paginationParameters.Page
        };
    }
}
