namespace RSM.Socar.CRM.Web.Security;

public interface IPermissionRegistry
{
    IReadOnlyList<string> GetAllPermissions();
}
