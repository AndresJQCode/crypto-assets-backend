using Domain.SeedWork;
using Microsoft.AspNetCore.Identity;

namespace Domain.AggregatesModel.UserAggregate;

public class UserLogin : IdentityUserLogin<Guid>, IAggregateRoot
{
}
