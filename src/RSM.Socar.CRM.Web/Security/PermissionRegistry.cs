namespace RSM.Socar.CRM.Web.Security;

public sealed class PermissionRegistry : IPermissionRegistry
{
    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);

    public void Register(string permission)
    {
        _permissions.Add(permission);
    }

    public IReadOnlyCollection<string> GetAllPermissions() => _permissions;
}
