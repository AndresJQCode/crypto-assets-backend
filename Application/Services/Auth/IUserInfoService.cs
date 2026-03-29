using Application.Dtos.Auth;
using Domain.AggregatesModel.UserAggregate;

namespace Application.Services.Auth;

internal interface IUserInfoService
{
    Task<AuthUserDto> GetUserInfoAsync(User user);
}
