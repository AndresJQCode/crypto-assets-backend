using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.EntityConfigurations;

public static class PermissionSeedData
{
    public static async Task SeedPermissionsAsync(ApiContext context)
    {
        // Verificar si ya existen permisos
        if (await context.Permissions.AnyAsync())
        {
            return;
        }

        // Crear permisos básicos para el sistema
        List<Permission>? permissions = new List<Permission>
        {
            // Permisos para Dashboard
            new("Ver Dashboard", "Permite ver el dashboard", "Dashboard", "Read"),
            new("Ver total de usuarios en Dashboard", "Permite ver  la cantidad de usuarios totales", "Dashboard", "ViewAllUsersCount"),
            new("Ver usuarios activos en Dashboard", "Permite ver la cantidad de usuarios activos", "Dashboard", "ViewActiveUsersCount"),
            new("Ver usuarios inactivos en Dashboard", "Permite ver la cantidad de usuarios inactivos", "Dashboard", "ViewInactiveUsersCount"),

            // Permisos para Usuarios
            new("Ver Usuarios", "Permite ver la lista de usuarios", "Users", "Read"),
            new("Crear Usuarios", "Permite crear nuevos usuarios", "Users", "Create"),
            new("Actualizar Usuarios", "Permite actualizar información de usuarios", "Users", "Update"),
            new("Eliminar Usuarios", "Permite eliminar usuarios", "Users", "Delete"),

            // Permisos para Roles
            new("Ver Roles", "Permite ver la lista de roles", "Roles", "Read"),
            new("Crear Roles", "Permite crear nuevos roles", "Roles", "Create"),
            new("Actualizar Roles", "Permite actualizar información de roles", "Roles", "Update"),
            new("Eliminar Roles", "Permite eliminar roles", "Roles", "Delete"),

            // Permisos para Permisos
            new("Ver Permisos", "Permite ver la lista de permisos", "Permissions", "Read"),
            new("Crear Permisos", "Permite crear nuevos permisos", "Permissions", "Create"),
            new("Actualizar Permisos", "Permite actualizar información de permisos", "Permissions", "Update"),
            new("Eliminar Permisos", "Permite eliminar permisos", "Permissions", "Delete"),
            new("Asignar Permisos", "Permite asignar permisos a usuarios", "Permissions", "Assign"),

            // Permisos para Settings
            new("Ver Configuración", "Permite ver la configuración del sistema", "Settings", "Read"),
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }

    public static async Task SeedPermissionsToRolesAsync(ApiContext context)
    {
        // Verificar si ya existen asignaciones de permisos a roles
        if (await context.PermissionRoles.AnyAsync())
        {
            return;
        }

        // Obtener todos los permisos y roles
        var permissions = await context.Permissions.ToListAsync();
        var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Administrador");
        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Usuario");

        var permissionRoles = new List<PermissionRole>();

        // SuperAdmin: todos los permisos
        if (superAdminRole != null)
        {
            permissionRoles.AddRange(permissions.Select(p => new PermissionRole(p.Id, superAdminRole.Id)));
        }

        // Admin: permisos de lectura y algunos de escritura (sin eliminar)
        if (adminRole != null)
        {
            var adminPermissions = permissions.Where(p =>
                p.Action == "Read" ||
                p.Action == "Create" ||
                p.Action == "Update" ||
                (p.Resource == "Users" && p.Action == "Assign")
            ).ToList();
            permissionRoles.AddRange(adminPermissions.Select(p => new PermissionRole(p.Id, adminRole.Id)));
        }

        // User: solo permisos de lectura básicos
        if (userRole != null)
        {
            var userPermissions = permissions.Where(p =>
                p.Action == "Read" &&
                (p.Resource == "Dashboard" || p.Resource == "Products")
            ).ToList();
            permissionRoles.AddRange(userPermissions.Select(p => new PermissionRole(p.Id, userRole.Id)));
        }

        await context.PermissionRoles.AddRangeAsync(permissionRoles);
        await context.SaveChangesAsync();
    }

    public static async Task SeedPermissionsToUsersAsync(ApiContext context)
    {
        // Ya no se asignan permisos directamente a usuarios
        // Los permisos se asignan únicamente a través de roles
        await Task.CompletedTask;
    }
}
