namespace RSM.Socar.CRM.Domain.Identity;

public sealed class UserPermission
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = default!;
}
