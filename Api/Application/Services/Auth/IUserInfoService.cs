using Api.Application.Dtos.Auth;
using Domain.AggregatesModel.UserAggregate;

namespace Api.Application.Services.Auth;

internal interface IUserInfoService
{
    Task<AuthUserDto> GetUserInfoAsync(User user);
}
