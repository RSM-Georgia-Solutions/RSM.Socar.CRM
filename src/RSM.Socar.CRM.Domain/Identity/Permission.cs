using RSM.Socar.CRM.Domain.Common;

namespace RSM.Socar.CRM.Domain.Identity;

public sealed class Permission : BaseEntity
{
    public string Name { get; set; } = default!;         // e.g. "User.Read"
    public string? Description { get; set; }

    // Many-to-many
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<UserPermission> UserPermissions { get; set; } = [];
}
