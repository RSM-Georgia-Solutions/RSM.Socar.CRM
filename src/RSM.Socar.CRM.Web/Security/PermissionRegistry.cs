namespace RSM.Socar.CRM.Web.Security;

public sealed class PermissionRegistry : IPermissionRegistry
{
    private readonly List<string> _permissions =
    [
        "User.Read",
        "User.Create",
        "User.Update",
        "User.Delete",
        "User.SetPassword"
        // Add more later → auto-generation can replace this.
    ];

    public IReadOnlyList<string> GetAllPermissions() => _permissions;
}
