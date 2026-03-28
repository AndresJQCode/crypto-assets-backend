using Domain.AggregatesModel.RoleAggregate;
using Domain.SeedWork;

namespace Domain.AggregatesModel.PermissionAggregate;

public class PermissionRole : Entity<Guid>, IAggregateRoot
{
    public Guid PermissionId { get; private set; }
    public Guid RoleId { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navegación
    public Permission Permission { get; private set; } = null!;
    public Role Role { get; private set; } = null!;


    public PermissionRole(Guid permissionId, Guid roleId)
    {
        Id = Guid.CreateVersion7();
        PermissionId = permissionId;
        RoleId = roleId;

        // Usar propiedades heredadas de Entity
        CreatedOn = DateTimeOffset.UtcNow;
    }
}
