using Api.Application.Dtos.Role;
using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.RoleAggregate;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Api.Application.Commands.RoleCommands;

internal sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionRoleRepository _permissionRoleRepository;

    public CreateRoleCommandHandler(
        RoleManager<Role> roleManager,
        IPermissionRepository permissionRepository,
        IPermissionRoleRepository permissionRoleRepository)
    {
        _roleManager = roleManager;
        _permissionRepository = permissionRepository;
        _permissionRoleRepository = permissionRoleRepository;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // 1. Verificar si ya existe un rol con ese nombre
        var existingRole = await _roleManager.FindByNameAsync(request.Name);
        if (existingRole != null)
        {
            throw new BadRequestException("Ya existe un rol con ese nombre");
        }

        // 2. Validar que todos los permisos existen (si se proporcionan)
        List<string>? permissionIds = request.PermissionIds?.ToList() ?? [];
        List<Permission>? validPermissions = [];

        foreach (var permissionId in permissionIds)
        {
            var permission = await _permissionRepository.GetById(Guid.Parse(permissionId), cancellationToken: cancellationToken) ?? throw new BadRequestException($"Permiso con ID {permissionId} no encontrado");
            validPermissions.Add(permission);
        }

        // 3. Crear el nuevo rol
        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        var createResult = await _roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Error al crear el rol: {errors}");
        }

        // 4. Asignar permisos al rol (si se proporcionan)
        if (permissionIds.Count > 0)
        {
            var permissionGuids = permissionIds.Select(Guid.Parse).ToList();
            await AssignPermissionsToRole(role.Id, permissionGuids, cancellationToken);
        }

        // 5. Retornar el rol creado
        return new RoleDto
        {
            Id = role.Id.ToString(),
            Name = role.Name,
            Description = role.Description
        };
    }

    private async Task AssignPermissionsToRole(Guid roleId, List<Guid> permissionIds, CancellationToken cancellationToken)
    {
        foreach (var permissionId in permissionIds)
        {
            // Verificar si ya existe una relación
            var existingPermissionRole = await _permissionRoleRepository.GetByRoleAndPermissionAsync(roleId, permissionId, cancellationToken);

            if (existingPermissionRole == null)
            {
                // Crear nueva relación PermissionRole
                var permissionRole = new PermissionRole(permissionId, roleId);
                await _permissionRoleRepository.Create(permissionRole, cancellationToken);
            }
        }

        // Guardar todos los cambios
        await _permissionRoleRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
