using Domain.SeedWork;

namespace Domain.AggregatesModel.PermissionAggregate;

public class Permission : Entity<Guid>, IAggregateRoot
{
    public Permission(string name, string description, string resource, string action, Guid? createdBy = null)
    {
        Id = Guid.CreateVersion7();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        Action = action ?? throw new ArgumentNullException(nameof(action));

        // Inicializar propiedades heredadas de Entity
        CreatedOn = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
    }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Resource { get; private set; }
    public string Action { get; private set; }

    // Navegación
    public ICollection<PermissionRole> PermissionRoles { get; private set; } = [];

    private Permission() : this(string.Empty, string.Empty, string.Empty, string.Empty) { } // Para EF Core

    public void Update(string name, string description, string resource, string action, Guid? modifiedBy = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        Action = action ?? throw new ArgumentNullException(nameof(action));
        LastModifiedBy = modifiedBy ?? null;
        LastModifiedOn = DateTimeOffset.UtcNow;
    }

    public string PermissionKey => $"{Resource}.{Action}";

}
