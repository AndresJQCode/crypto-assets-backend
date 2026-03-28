using Domain.SeedWork;
using Microsoft.AspNetCore.Identity;

namespace Domain.AggregatesModel.UserAggregate;

public class RoleClaim : IdentityRoleClaim<Guid>, IAggregateRoot
{
}
