using Domain.SeedWork;
using Microsoft.AspNetCore.Identity;

namespace Domain.AggregatesModel.UserAggregate;

public class User : IdentityUser<Guid>, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public ICollection<UserRole> UserRoles { get; init; } = [];

    public User()
    {
        Id = Guid.CreateVersion7();
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
