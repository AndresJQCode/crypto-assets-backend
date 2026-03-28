using MediatR;

namespace Api.Application.Commands.RoleCommands
{
    internal sealed class RemoveUserFromRoleCommand : IRequest<bool>
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;

        public RemoveUserFromRoleCommand(string userId, string roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}
