using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.SeedWork;
using Microsoft.AspNetCore.Identity;

namespace Domain.AggregatesModel.RoleAggregate;

public class Role : IdentityRole<Guid>, IAggregateRoot
{
    public string? Description { get; set; } // Descripción opcional del rol
    public ICollection<UserRole> UserRoles { get; init; } = []; // Propiedad para la relación UserRoles
    public ICollection<PermissionRole> PermissionRoles { get; init; } = []; // Propiedad para la relación PermissionRoles

    public Role() => Id = Guid.CreateVersion7();
}
