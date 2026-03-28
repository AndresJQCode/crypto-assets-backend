using Domain.AggregatesModel.RoleAggregate;
using Domain.SeedWork;
using Microsoft.AspNetCore.Identity;

namespace Domain.AggregatesModel.UserAggregate;

public class UserRole : IdentityUserRole<Guid>, IAggregateRoot
{
}
