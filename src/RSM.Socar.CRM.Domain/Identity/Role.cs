using RSM.Socar.CRM.Domain.Common;
using RSM.Socar.CRM.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSM.Socar.CRM.Domain.Identity;

public sealed class Role : AuditableEntity
{
    public string Name { get; set; } = default!;         // e.g. "Admin"
    public string? Description { get; set; }

    public RoleStatus Status { get; set; }

    [NotMapped]
    public int UsersCount => UserRoles?.Count ?? 0;

    [NotMapped]
    public int PermissionsCount => RolePermissions?.Count ?? 0;


    // Many-to-many
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
