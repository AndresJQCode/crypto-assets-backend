using System.Security.Claims;
using Api.Application.Dtos.Permission;
using Api.Application.Dtos.User;
using Api.Application.Queries.Users;
using Api.Application.Services;
using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using Infrastructure;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Queries.UsersQueries;

internal sealed class GetCurrentUserQueryHandler(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, ApiContext context, IUserPermissionService userPermissionService, ILogger<GetCurrentUserQueryHandler> logger) : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User?.Identity?.IsAuthenticated != true)
        {
            throw new UnAuthorizedException();
        }

        string? userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnAuthorizedException();
        }

        User? user = await userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Usuario no encontrado");
        }

        string[]? userRoles = await (from ur in context.UserRoles
                                     join r in context.Roles on ur.RoleId equals r.Id
                                     where ur.UserId == user.Id
                                     select r.Name
                             ).ToArrayAsync(cancellationToken);
        try
        {
            IEnumerable<Permission>? permissions = await userPermissionService.GetUserPermissionsAsync(user.Id, cancellationToken);
            CurrentUserDto currentUserDto = new CurrentUserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                Name = user.Name ?? string.Empty,
                IsActive = user.IsActive,
                TenantId = user.TenantId?.ToString(),
                Roles = [.. userRoles],
                Permissions = [.. permissions.Select(p => new UserPermissionDto
                    {
                        PermissionKey = p.PermissionKey,
                        Resource = p.Resource,
                        Action = p.Action
                    })]
            };

            return currentUserDto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting current user {UserId}", userId);
            throw;
        }
    }
}
