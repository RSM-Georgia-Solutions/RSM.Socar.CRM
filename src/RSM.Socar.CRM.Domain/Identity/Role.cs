using RSM.Socar.CRM.Domain.Common;

namespace RSM.Socar.CRM.Domain.Identity;

public sealed class Role : BaseEntity
{
    public string Name { get; set; } = default!;         // e.g. "Admin"
    public string? Description { get; set; }

    // Many-to-many
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
