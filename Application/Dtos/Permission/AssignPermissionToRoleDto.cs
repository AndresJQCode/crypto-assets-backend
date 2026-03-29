namespace Application.Dtos.Permission
{
    internal sealed class AssignPermissionToRoleDto
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
    }
}
