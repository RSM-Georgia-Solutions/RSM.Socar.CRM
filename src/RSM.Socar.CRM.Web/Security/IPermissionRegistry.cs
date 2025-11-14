
namespace RSM.Socar.CRM.Web.Security;

public interface IPermissionRegistry
{
    IReadOnlyCollection<string> GetAllPermissions();
}
