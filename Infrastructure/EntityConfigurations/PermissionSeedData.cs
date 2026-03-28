using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Infrastructure.Constants;
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
            new("Ver Dashboard", "Permite ver el dashboard", PermissionResourcesConstants.Dashboard, PermissionActionsConstants.Read),
            new("Ver total de usuarios en Dashboard", "Permite ver la cantidad de usuarios totales", PermissionResourcesConstants.Dashboard, "ViewAllUsersCount"),
            new("Ver usuarios activos en Dashboard", "Permite ver la cantidad de usuarios activos", PermissionResourcesConstants.Dashboard, "ViewActiveUsersCount"),
            new("Ver usuarios inactivos en Dashboard", "Permite ver la cantidad de usuarios inactivos", PermissionResourcesConstants.Dashboard, "ViewInactiveUsersCount"),

            // Permisos para Usuarios
            new("Ver Usuarios", "Permite ver la lista de usuarios", PermissionResourcesConstants.Users, PermissionActionsConstants.Read),
            new("Crear Usuarios", "Permite crear nuevos usuarios", PermissionResourcesConstants.Users, PermissionActionsConstants.Create),
            new("Actualizar Usuarios", "Permite actualizar información de usuarios", PermissionResourcesConstants.Users, PermissionActionsConstants.Update),
            new("Eliminar Usuarios", "Permite eliminar usuarios", PermissionResourcesConstants.Users, PermissionActionsConstants.Delete),

            // Permisos para Roles
            new("Ver Roles", "Permite ver la lista de roles", PermissionResourcesConstants.Roles, PermissionActionsConstants.Read),
            new("Crear Roles", "Permite crear nuevos roles", PermissionResourcesConstants.Roles, PermissionActionsConstants.Create),
            new("Actualizar Roles", "Permite actualizar información de roles", PermissionResourcesConstants.Roles, PermissionActionsConstants.Update),
            new("Eliminar Roles", "Permite eliminar roles", PermissionResourcesConstants.Roles, PermissionActionsConstants.Delete),

            // Permisos para Permisos
            new("Ver Permisos", "Permite ver la lista de permisos", PermissionResourcesConstants.Permissions, PermissionActionsConstants.Read),
            new("Crear Permisos", "Permite crear nuevos permisos", PermissionResourcesConstants.Permissions, PermissionActionsConstants.Create),
            new("Actualizar Permisos", "Permite actualizar información de permisos", PermissionResourcesConstants.Permissions, PermissionActionsConstants.Update),
            new("Eliminar Permisos", "Permite eliminar permisos", PermissionResourcesConstants.Permissions, PermissionActionsConstants.Delete),
            new("Asignar Permisos", "Permite asignar permisos a usuarios", PermissionResourcesConstants.Permissions, PermissionActionsConstants.Assign),

            // Permisos para Settings
            new("Ver Configuración", "Permite ver la configuración del sistema", "Settings", PermissionActionsConstants.Read),

            // Permisos para Tenants (solo SuperAdmin)
            new("Ver Tenants", "Permite ver la lista de tenants", PermissionResourcesConstants.Tenants, PermissionActionsConstants.Read),
            new("Crear Tenants", "Permite crear nuevos tenants", PermissionResourcesConstants.Tenants, PermissionActionsConstants.Create),
            new("Actualizar Tenants", "Permite actualizar información de tenants", PermissionResourcesConstants.Tenants, PermissionActionsConstants.Update),
            new("Eliminar Tenants", "Permite eliminar tenants", PermissionResourcesConstants.Tenants, PermissionActionsConstants.Delete),

            // Permisos para Connector Definitions (solo SuperAdmin)
            new("Ver Definiciones de Conectores", "Permite ver el catálogo de conectores disponibles", PermissionResourcesConstants.ConnectorDefinitions, PermissionActionsConstants.Read),
            new("Crear Definiciones de Conectores", "Permite agregar nuevos tipos de conectores al catálogo", PermissionResourcesConstants.ConnectorDefinitions, PermissionActionsConstants.Create),
            new("Actualizar Definiciones de Conectores", "Permite actualizar información de conectores en el catálogo", PermissionResourcesConstants.ConnectorDefinitions, PermissionActionsConstants.Update),
            new("Eliminar Definiciones de Conectores", "Permite eliminar tipos de conectores del catálogo", PermissionResourcesConstants.ConnectorDefinitions, PermissionActionsConstants.Delete),
            new("Activar/Desactivar Definiciones de Conectores", "Permite activar o desactivar conectores en el catálogo", PermissionResourcesConstants.ConnectorDefinitions, PermissionActionsConstants.Activate),

            // Permisos para Connector Instances (Tenants)
            new("Ver Conectores", "Permite ver la lista de conectores configurados", PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Read),
            new("Crear Conectores", "Permite configurar nuevos conectores (Shopify, WooCommerce, etc.)", PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Create),
            new("Actualizar Conectores", "Permite actualizar configuración de conectores", PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Update),
            new("Eliminar Conectores", "Permite eliminar conectores configurados", PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Delete),
            new("Habilitar/Deshabilitar Conectores", "Permite habilitar o deshabilitar conectores", PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Enable),
            new("Reautorizar Conectores", "Permite reautorizar conectores OAuth (nuevos permisos)", PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Reauthorize),
            new("Validar Conectores", "Permite validar que la conexión de un conector esté activa", PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Validate),

            // Permisos para System Configuration (solo SuperAdmin)
            new("Ver Configuración del Sistema", "Permite ver configuraciones operacionales del sistema", PermissionResourcesConstants.SystemConfiguration, PermissionActionsConstants.Read),
            new("Actualizar Configuración del Sistema", "Permite modificar configuraciones operacionales del sistema", PermissionResourcesConstants.SystemConfiguration, PermissionActionsConstants.Update),
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
        List<Permission>? permissions = await context.Permissions.ToListAsync();

        List<PermissionRole>? permissionRoles = [];

        // SuperAdmin: todos los permisos excepto ConnectorInstances (son por tenant)
        Role? superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RolesEnum.SuperAdmin.ToString());
        if (superAdminRole is not null)
        {
            var superAdminPermissions = permissions.Where(p => p.Resource != PermissionResourcesConstants.ConnectorInstances).ToList();
            permissionRoles.AddRange(superAdminPermissions.Select(p => new PermissionRole(p.Id, superAdminRole.Id)));
        }

        Role? adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RolesEnum.Admin.ToString());
        if (adminRole is not null)
        {
            // agregar todos los permisos del recurso Dashboard
            List<PermissionRole>? dashboardPermissions = permissions.Where(p => p.Resource == PermissionResourcesConstants.Dashboard).Select(p => new PermissionRole(p.Id, adminRole.Id)).ToList();
            permissionRoles.AddRange(dashboardPermissions);

            // agregar todos los permisos del recurso Users
            List<PermissionRole>? usersPermissions = permissions.Where(p => p.Resource == PermissionResourcesConstants.Users).Select(p => new PermissionRole(p.Id, adminRole.Id)).ToList();
            permissionRoles.AddRange(usersPermissions);

            // agregar todos los permisos del recurso Roles
            List<PermissionRole>? rolesPermissions = permissions.Where(p => p.Resource == PermissionResourcesConstants.Roles).Select(p => new PermissionRole(p.Id, adminRole.Id)).ToList();
            permissionRoles.AddRange(rolesPermissions);

            // agregar todos los permisos de ConnectorInstances
            List<PermissionRole>? connectorInstancesPermissions = permissions.Where(p => p.Resource == PermissionResourcesConstants.ConnectorInstances).Select(p => new PermissionRole(p.Id, adminRole.Id)).ToList();
            permissionRoles.AddRange(connectorInstancesPermissions);
        }

        Role? userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RolesEnum.User.ToString());
        if (userRole is not null)
        {
            // agregar todos los permisos del recurso Dashboard
            List<PermissionRole>? dashboardPermissions = permissions.Where(p => p.Resource == PermissionResourcesConstants.Dashboard).Select(p => new PermissionRole(p.Id, userRole.Id)).ToList();
            permissionRoles.AddRange(dashboardPermissions);
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
