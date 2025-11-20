using RSM.Socar.CRM.Domain.Common;
using RSM.Socar.CRM.Domain.Enums;

namespace RSM.Socar.CRM.Domain.Identity;

public sealed class Permission : AuditableEntity
{
    public required string Ketworrd { get; set; }
    public string Name { get; set; } = default!;         // e.g. "User.Read"
    public string? Description { get; set; }
    public PermissionStatus Status { get; set; }

    // Many-to-many
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
