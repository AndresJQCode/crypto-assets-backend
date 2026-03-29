using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.EntityConfigurations;

public static class SeedDb
{
    public static async Task SeedData(ApiContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync().ConfigureAwait(false))
        {
            return;
        }

        // Inicializar permisos primero
        await PermissionSeedData.SeedPermissionsAsync(context);

        await SeedUsers(context).ConfigureAwait(false);

        // asignar permisos a roles
        await PermissionSeedData.SeedPermissionsToRolesAsync(context);

        // asignar permisos a usuarios (mantener para compatibilidad)
        await PermissionSeedData.SeedPermissionsToUsersAsync(context);

    }

    private static async Task SeedUsers(ApiContext context)
    {
        var roleSuperAdmin = new Role() { Name = RolesEnum.SuperAdmin.ToString(), NormalizedName = RolesEnum.SuperAdmin.ToString().ToUpper(System.Globalization.CultureInfo.CurrentCulture) };
        var roleAdmin = new Role() { Name = RolesEnum.Admin.ToString(), NormalizedName = RolesEnum.Admin.ToString().ToUpper(System.Globalization.CultureInfo.CurrentCulture) };
        var roleUser = new Role() { Name = RolesEnum.User.ToString(), NormalizedName = RolesEnum.User.ToString().ToUpper(System.Globalization.CultureInfo.CurrentCulture) };

        context.Roles.AddRange(
            roleSuperAdmin,
            roleAdmin,
            roleUser
        );

        await context.SaveChangesAsync().ConfigureAwait(false);


        string emailAdmin = "andres.jaramillo@qcode.co";
        var userAdmin = new User()
        {
            Email = emailAdmin,
            Name = "Andres Jaramillo",
            UserName = emailAdmin,
            EmailConfirmed = true,
            ConcurrencyStamp = "1",
            SecurityStamp = "1",
            NormalizedEmail = emailAdmin.ToUpperInvariant(),
            NormalizedUserName = emailAdmin.ToUpperInvariant()
        };

        userAdmin.PasswordHash = new PasswordHasher<User>().HashPassword(userAdmin, "Debian7194*");
        context.Users.Add(userAdmin);
        await context.SaveChangesAsync();

        var userRole = new UserRole() { RoleId = roleSuperAdmin.Id, UserId = userAdmin.Id };
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }
}
